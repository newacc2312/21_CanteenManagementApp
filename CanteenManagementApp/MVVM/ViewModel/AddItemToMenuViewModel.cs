﻿using CanteenManagementApp.Core;
using CanteenManagementApp.MVVM.Model;
using CanteenManagementApp.MVVM.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace CanteenManagementApp.MVVM.ViewModel
{
    public class AddItemToMenuViewModel : ObservableObject
    {
        public ObservableCollection<Item> FoodItems { get; set; }
        private readonly CollectionViewSource FoodItemsCollection;
        public ICollectionView FoodSourceCollection => FoodItemsCollection.View;
        public ICommand AddItemToMenuOkCommand { get; set; } // nút thêm hàng vào menu
                                                             //
        public ObservableCollection<Item> FoodItemsOld { get; set; }
        public AddItemToMenuViewModel()
        {

            FoodItems = new ObservableCollection<Item>(DbQueries.ItemQueries.GetItemsByType(0));
            //{
            //    //new Item() { Type = 0, Name = "Gà nướng", Amount = 10, Description = "Món ức gà chiên mắm cay có màu sắc hấp dẫn, vị mặn mặn chua chua của nước sốt giúp thịt gà tăng thêm hương vị và kích thích vị giác người ăn. vị đậm đà của mùi nước mắm quyện vào thịt, và vị bùi bùi của tỏi, cay nồng của ớt sừng. Món này ăn kèm cơm nóng, hoặc bánh mì tùy thích.\r\n\r\n\r\n", Id = 10, Price = 12000},
            //    //new Item() { Type = 0, Name = "Lòng bò xào gà", Amount = 10, Description = "Ngon", Id = 11, Price = 12000},
            //    //new Item() { Type = 0, Name = "Lòng heo xào gà", Amount = 10, Description = "Ngon", Id = 12, Price = 12000},
            //    //new Item() { Type = 0, Name = "Lòng xào gà", Amount = 10, Description = "Ngon", Id = 13, Price = 12000 },
            //    //new Item() { Type = 0, Name = "Lòng xào gà", Amount = 10, Description = "Ngon", Id = 14, Price = 12000},
            //    //new Item() { Type = 0, Name = "Lòng xào gà", Amount = 10, Description = "Ngon", Id = 15, Price = 12000},
            //    //new Item() { Type = 0, Name = "Gà nướng", Amount = 10, Description = "Ngon", Id = 10, Price = 12000},
            //    //new Item() { Type = 0, Name = "Lòng bò xào gà", Amount = 10, Description = "Ngon", Id = 11, Price = 12000 }
            //};

            //using var dbContext = new CanteenContext();
            //var items = dbContext.Items;
            //foreach (var item in items)
            //{
            //    FoodItems.Add(item);
            //}

           FoodItemsOld = new ObservableCollection<Item>(DbQueries.MenuQueries.GetTodayMenuItems());


            for (int i = 0; i < FoodItemsOld.Count; i++)
            {
                for (int j = 0; j < FoodItems.Count; j++)
                {
                    if (FoodItemsOld[i].Name.Equals(FoodItems[j].Name)
                        && FoodItemsOld[i].Price == FoodItems[j].Price
                        && FoodItemsOld[i].Description.Equals(FoodItems[j].Description))
                    {
                        FoodItems.RemoveAt(j);
                    }
                }
            }

            FoodItemsCollection = new CollectionViewSource { Source = FoodItems };
            AddItemToMenuOkCommand = new RelayCommand<AddItemToMenu>((parameter) => true, (parameter) => AddItemToMenuOk(parameter));
            
        }
      
        public static void AddItemToMenuOk(AddItemToMenu parameter)
        {
            parameter.DialogResult = true;
        }
      
    }
}