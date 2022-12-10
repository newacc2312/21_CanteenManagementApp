﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CanteenManagementApp.MVVM.Model
{
    public static class DbQueries
    {
        public static class CustomerQueries
        {
            /* Query */

            public static Customer GetCustomerById(string customerId)
            {
                using var context = new CanteenContext();

                var customer = context.Customers
                                        .Where(c => c.Id == customerId)
                                        .FirstOrDefault();
                return customer;
            }

            public static async Task<List<Item>> GetFrequentlyBoughtItemsByCustomerIdAsync(string customerId)
            {
                using var context = new CanteenContext();
                var items = await context.Customers
                                    .Where(c => c.Id == customerId)
                                    .Join(context.Receipts, c => c.Id, r => r.CustomerId, (c, r) => new { r.Id })
                                    .Join(context.Receipt_Items, cr => cr.Id, ri => ri.ReceiptId, (cr, ri) => new { ri.Item, ri.Amount })
                                    .GroupBy(crri => crri.Item)
                                    .Select(itemGroup => new
                                    {
                                        Item = itemGroup.Key,
                                        Sum = itemGroup.Sum(crri => crri.Amount)
                                    })
                                    .OrderByDescending(i => i.Sum)
                                    .Select(i => i.Item)
                                    .Take(5)
                                    .ToListAsync();
                return items;
            }

            /* Insert */

            public static async Task InsertCustomerAsync(string customerId, string customerName, string customerType)
            {
                using var context = new CanteenContext();

                await context.Customers.AddAsync(new Customer
                {
                    Id = customerId,
                    Name = customerName,
                    CustomerType = customerType,
                    Balance = 0
                });

                int rows = await context.SaveChangesAsync();
                Debug.WriteLine($"Saved {rows} customers");
            }

            /* Update */

            public static async Task TopUpCustomerBalance(Customer customer, float amount)
            {
                using var context = new CanteenContext();

                customer.Balance += amount;

                context.Customers.Update(customer);

                int rows = await context.SaveChangesAsync();

                Debug.WriteLine($"{rows} rows in Customer updated");
            }

            public static async Task UpdateCustomerBalanceOnPurchase(Customer customer, float orderCost)
            {
                using var context = new CanteenContext();

                customer.Balance -= orderCost;

                context.Customers.Update(customer);

                int rows = await context.SaveChangesAsync();

                Debug.WriteLine($"{rows} rows in Customer updated");
            }

            /* Delete */
        }

        public static class ItemQueries
        {
            /* Query */

            public static List<Item> GetItemsByType(int itemType)
            {
                using var context = new CanteenContext();

                var items = context.Items
                                .Where(i => i.Type == itemType)
                                .ToList();

                return items;
            }

            public static Item GetItemById(int itemId)
            {
                using var context = new CanteenContext();

                var item = context.Items
                                .Where(i => i.Id == itemId)
                                .FirstOrDefault();

                return item;
            }

            /* Insert */

            public static async Task InsertItemAsync(int itemType, string itemName, float itemPrice, string description = "", int amount = 0)
            {
                using var context = new CanteenContext();

                await context.Items.AddAsync(new Item
                {
                    Type = itemType,
                    Name = itemName,
                    Price = itemPrice,
                    Description = description,
                    Amount = amount
                });

                int rows = await context.SaveChangesAsync();
                Debug.WriteLine($"Saved {rows} items");
            }

            public static async Task InsertItemAsync(Item item, bool identityInsert = false)
            {
                using var context = new CanteenContext();
                using var transaction = context.Database.BeginTransaction();
                await context.Items.AddAsync(item);

                int rows;
                if (identityInsert)
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Item ON;");
                    rows = await context.SaveChangesAsync();
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Item OFF;");
                    transaction.Commit();
                }
                else
                {
                    rows = await context.SaveChangesAsync();
                }
                Debug.WriteLine($"Saved {rows} items");
            }

            /* Update */

            public static void UpdateItem(Item item)
            {
                using var context = new CanteenContext();

                Item oldItem = GetItemById(item.Id);
                oldItem.Name = item.Name;
                oldItem.Price = item.Price;
                oldItem.Description = item.Description;
                oldItem.Amount = item.Amount;

                int rows = context.SaveChanges();
                Debug.WriteLine($"{rows} items updated");
            }

            /* Delete */

            public static void DeleteItemById(int itemId)
            {
                using var context = new CanteenContext();

                Item item = GetItemById(itemId);
                context.Items.Remove(item);

                int rows = context.SaveChanges();
                Debug.WriteLine($"{rows} items deleted");
            }
        }

        public static class MenuQueries
        {
            /* Query */

            /* Insert */

            /* Update */

            /* Delete */
        }

        public static class ReceiptQueries
        {
            /* Query */

            public static Receipt GetReceiptById(int receiptId)
            {
                using var context = new CanteenContext();

                var receipt = context.Receipts
                                        .Where(r => r.Id == receiptId)
                                        .FirstOrDefault();

                return receipt;
            }

            public static List<Receipt> GetReceiptsByCustomerId(string customerId)
            {
                using var context = new CanteenContext();

                var receipts = context.Receipts
                                        .Where(r => r.Customer.Id == customerId)
                                        .ToList();

                return receipts;
            }

            public static List<ItemOrder> GetReceiptDetailsByReceipt(Receipt receipt)
            {
                using var context = new CanteenContext();

                var itemOrders = context.Receipt_Items
                                            .Join(context.Items, ri => ri.ItemId, i => i.Id, (ri, i) => new { ri, i })
                                            .Where(rii => rii.ri.ReceiptId.Equals(receipt.Id))
                                            .Select(rii => new ItemOrder { Item = rii.i, Amount = rii.ri.Amount })
                                            .ToList();
                return itemOrders;
            }

            /* Insert */

            public static async Task<int> InsertReceiptAsync(string customerId, List<ItemOrder> item_orders, string paymentMethod, float total)
            {
                using var context = new CanteenContext();

                var datetime = DateTime.Now;

                Receipt receipt = new()
                {
                    CustomerId = customerId,
                    PaymentMethod = paymentMethod,
                    DateTime = datetime,
                    Total = total
                };
                context.Receipts.Add(receipt);

                int rows = context.SaveChanges();
                Debug.WriteLine($"Saved {rows} receipts");

                int receiptId = receipt.Id;

                foreach (ItemOrder item_order in item_orders)
                {
                    await ReceiptItemQueries.InsertReceiptItemAsync(receiptId, item_order.Item.Id, item_order.Amount);
                }
                return receiptId;
            }

            /* Update */

            /* Delete */
        }

        public static class ReceiptItemQueries
        {
            /* Query */

            /* Insert */

            public static async Task InsertReceiptItemAsync(int receiptId, int itemId, int amount)
            {
                using var context = new CanteenContext();

                await context.Receipt_Items.AddAsync(new Receipt_Item
                {
                    ReceiptId = receiptId,
                    ItemId = itemId,
                    Amount = amount
                });

                int rows = await context.SaveChangesAsync();
                Debug.WriteLine($"Saved {rows} receipt_items");
            }

            /* Update */

            /* Delete */
        }
    }
}