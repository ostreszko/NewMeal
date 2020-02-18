using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Callorie.Models;
using Callorie.SQL;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Callorie.Controllers
{
    public class NewMealController : Controller
    {
        //Obiekty klas odpowiadających za dostęp do bazy danych
        private readonly ItemContext _contextItem;
        private readonly SQLDish _dishDriver;
        private readonly SQLMeal _mealDriver;

        //Listy
        private List<DishItem> dishList = new List<DishItem>();
        private List<MealItem> mealList = new List<MealItem>();

        public NewMealController(ItemContext context)
        {
            _contextItem = context;
            _dishDriver = new SQLDish(_contextItem);
            _mealDriver = new SQLMeal(_contextItem);
        }

        //Wywołanie widoku bez przekazanych danych w modelu
        [HttpGet]
        public IActionResult NewMealView()
        {
            long loggedUserId = GetUserSessionId();
            NewMealModel returnModel = new NewMealModel();
            SetBasicInfo(ref dishList, ref mealList, ref returnModel, loggedUserId);
            returnModel.AllMeals = returnModel.AllMeals.Where(x => x.User_Id == loggedUserId).ToList();
            return View(returnModel);
        }
        //Wywołanie widoku z przekazanymi danymi w modelu
        [HttpPost]
        public IActionResult NewMealView(string submitButton,NewMealModel model)
        {
            long loggedUserId = GetUserSessionId();
            NewMealModel returnModel = new NewMealModel();
            SetBasicInfo(ref dishList, ref mealList,ref returnModel, loggedUserId);
            var meal = new MealItem();

            //Został wciśnięty przycisk "Dodaj posiłek"
            if (submitButton == "add")
            {
                //Jeżeli nie uzupełniono obowiązkowych pól formularza
                if (!ModelState.IsValid)
                {
                    return View(returnModel);
                }

                meal.Id = CalculateNextMealId();

                //Pobranie z bazy danych dania którego dotyczył posiłek
                DishItem selectedDish = _dishDriver.GetDish(model.DishId);

                //Uzupełnienie obiektu posiłku i dodanie go do bazy
                meal.User_Id = GetUserSessionId();
                meal.Dish_Id = model.DishId;
                meal.Amount = model.Amount;
                meal.Description = model.Description;
                meal.Meal_Name = model.Name;
                meal.Energy_Value = selectedDish.Energy_Value / 100 * model.Amount;
                meal.Carbonhydrate = selectedDish.Carbonhydrate / 100 * model.Amount;
                meal.Fat = selectedDish.Fat / 100 * model.Amount;
                meal.Fibre = selectedDish.Fibre / 100 * model.Amount;
                meal.Protein = selectedDish.Protein / 100 * model.Amount;
                meal.Sodium = selectedDish.Sodium / 100 * model.Amount;
                meal.Meal_Date = Convert.ToDateTime(model.Date +" "+ model.Time);

                _mealDriver.Add(meal);
                returnModel.AddedDish = model.Name;
                returnModel.Communicate = "Added";
            }
            //Został wciśnięty przycisk "Edytuj posiłek"
            else if (submitButton == "edit")
            {
                //Pobranie z bazy danych dania którego dotyczył posiłek
                DishItem selectedDish = _dishDriver.GetDish(model.EditedToDishId);

                //Uzupełnienie obiektu posiłku i aktualizacja odpowiedniego rekordu
                var mealEdit = new MealItem();
                mealEdit.Id = model.EditMealId;
                mealEdit.Meal_Name = model.EditedToName;
                mealEdit.Description = model.EditedToDescription;
                mealEdit.User_Id = GetUserSessionId();
                mealEdit.Dish_Id = model.EditedToDishId;
                mealEdit.Amount = model.EditedToAmount;
                mealEdit.Energy_Value = selectedDish.Energy_Value / 100 * model.EditedToAmount;
                mealEdit.Carbonhydrate = selectedDish.Carbonhydrate / 100 * model.EditedToAmount;
                mealEdit.Fat = selectedDish.Fat / 100 * model.EditedToAmount;
                mealEdit.Fibre = selectedDish.Fibre / 100 * model.EditedToAmount;
                mealEdit.Protein = selectedDish.Protein / 100 * model.EditedToAmount;
                mealEdit.Sodium = selectedDish.Sodium / 100 * model.EditedToAmount;
                mealEdit.Meal_Date = Convert.ToDateTime(model.EditedToDate + " " + model.EditedToTime);

                _mealDriver.Update(mealEdit);
                returnModel.AddedDish = model.EditedToName;

                returnModel.AllMeals = _mealDriver.GetAllMeal().ToList();
                returnModel.Communicate = "Edited";
            }
            //Został wciśnięty przycisk "Usuń posiłek"
            else if (submitButton == "delete")
            {
                //Usuwanie posiłku z bazy
                _mealDriver.Delete(model.EditMealId);
                returnModel.Communicate = "Deleted";
            }
            //Aktualizacja modelu zwrotnego po dodaniu/edycji/usunięciu posiłku
            SetBasicInfo(ref dishList, ref mealList, ref returnModel, loggedUserId);

            return View(returnModel);
        }

        //Metoda wybierająca ID użytkownika z sesji
        private long GetUserSessionId()
        {
            byte[] byteLoggedUserId = new byte[7000];
            HttpContext.Session.TryGetValue("LoggedUserId", out byteLoggedUserId);
            long userId = BitConverter.ToInt64(byteLoggedUserId);
            return userId;
        }
        //Metoda uzupełniająca pola modelu, aktualnymi danymi z bazy
        private void SetBasicInfo(ref List<DishItem> dishList, ref List<MealItem> mealList, ref NewMealModel returnModel, long loggedUserId)
        {
            dishList = _dishDriver.GetAllDish().ToList();
            mealList = _mealDriver.GetAllMeal().ToList();
            returnModel.OnlyMineDDLDishList = dishList.Where(x => x.UserId == loggedUserId).Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
            returnModel.MealList = mealList.Where(a => a.User_Id == loggedUserId).Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Meal_Name + " " + a.Meal_Date }).ToList();
            returnModel.AllMeals = mealList.ToList();
            returnModel.DDLDishList = dishList.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();
        }

        //Metoda zwracająca ID jakie powinien otrzymać kolejny dodawany na bazę posiłek
        private long CalculateNextMealId()
        {
            var allMeals = _mealDriver.GetAllMeal().ToList();
            List<long> maxIdList = new List<long>();
            if (allMeals.Count() > 0)
            {
                foreach (MealItem key in allMeals)
                {
                    maxIdList.Add(key.Id);
                }
                return maxIdList.Max() + 1;
            }
            else
            {
                return 1;
            }
        }
    }
}