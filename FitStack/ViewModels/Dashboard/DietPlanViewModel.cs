namespace FitStack.ViewModels.Dashboard
{
    public class DietPlanViewModel
    {
        public int Id { get; set; }
        public string Meal { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fats { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
