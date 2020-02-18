using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Callorie.Models
{
    //Model danych widoku
    public class NewMealModel
    {
        [Required(ErrorMessage = "Pole jest wymagane")]
        [DataType(DataType.Text,ErrorMessage ="Błędna wartość")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DishItem Dish { get; set; }

        public List<MealItem> AllMeals { get; set; }

        public int DishId { get; set; }

        public int MealId { get; set; }

        [Required(ErrorMessage ="Pole 'Ilosc' jest wymagane")]
        [DataType(DataType.Currency,ErrorMessage = "Wartość nie jest odpowiednia dla pola 'Ilość'")]
        public double Amount { get; set; }

        public List<SelectListItem> DDLDishList { get; set; }

        public List<SelectListItem> OnlyMineDDLDishList { get; set; }

        public List<SelectListItem> MealList { get; set; }

        public string AddedDish { get; set;}

        public string Time { get; set; }

        public string Date { get; set; }

        public int EditMealId { get; set; }

        public int EditedToDishId { get; set; }
        public double EditedToAmount { get; set; }

        public string EditedToDescription { get; set; }

        public string EditedToName { get; set; }

        public string EditedToTime { get; set; }

        public string EditedToDate { get; set; }

        public string Communicate { get; set; }
    }
}