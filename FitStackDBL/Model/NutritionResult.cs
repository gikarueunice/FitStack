using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitStackDBL.Model
{
    public class NutritionResult
    {
        public string MealName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
        public int Fiber { get; set; }
        public int Sugar { get; set; }
        public double ConfidenceScore { get; set; }
        public int HealthScore { get; set; }
        public string? Rationale { get; set; }
        public List<FoodComponent>? Components { get; set; }
    }
}
