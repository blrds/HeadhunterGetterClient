using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadhunterGetterClient.Models
{
    class ShortVacancy
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public int IdArea { get; set; }
        public string IdExperience { get; set; }
        public decimal SalaryFrom { get; set; }
        public decimal SalaryTo { get; set; }
        public string SalaryCurrency { get; set; }
        public DateTime PublishedDate { get; set; }
        public string SnippetRequirement { get; set; }
        public string SnippetResponsibility { get; set; }
        public string Description { get; set; }

        public ShortVacancy(FullVacancy full)
        {
            Id = full.Id;
            Name = full.Name;
            IdArea = full.Area.Id;
            IdExperience = full.Experience.Id;
            SalaryFrom = decimal.Parse(full.Salary.From);
            SalaryTo = decimal.Parse(full.Salary.To);
            SalaryCurrency = full.Salary.Currency;
            PublishedDate = full.Published_at;
            SnippetRequirement = full.Snippet.Requirement;
            SnippetResponsibility = full.Snippet.Responsibility;
            Description = full.Description;
        }
        public ShortVacancy() { }
    }
}
