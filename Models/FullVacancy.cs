using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HeadhunterGetterClient.Models
{
    public class VacancySkill
    {
        public int IdVacancy { get; set; }
        public int IdSkill { get; set; }
    }
    public class VacancySpecialization
    {
        public int IdVacancy { get; set; }
        public string IdSpecialization { get; set; }
    }
    class FullVacancy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Area Area { get; set; }
        public Experience Experience { get; set; }
        public Salary Salary { get; set; }
        public DateTime Published_at { get; set; }
        public Snippet Snippet { get; set; }
        public IEnumerable<Specialization> specializations { get; set; }
        public IEnumerable<Skill> Key_skills { get; set; }
        public string Description { get; set; }

        public FullVacancy(ShortVacancy @short)
        {
            Id = @short.Id;
            Name = @short.Name;
            Salary = new Salary();
            Salary.From = @short.SalaryFrom.ToString();
            Salary.To = @short.SalaryTo.ToString();
            Salary.Currency = @short.SalaryCurrency;
            Published_at = @short.PublishedDate;
            Snippet = new Snippet();
            Snippet.Requirement = @short.SnippetRequirement;
            Snippet.Responsibility = @short.SnippetResponsibility;
            Description = @short.Description;
            Key_skills = new List<Skill>();
            specializations = new List<Specialization>();

            #region settings
            string URL = "https://localhost:44309/api";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            #endregion
            #region skills
            client.BaseAddress = new Uri(URL + "/" + "vskills/vacancy/" + Id.ToString());
            var responce = client.GetAsync("").Result;
            if (responce.IsSuccessStatusCode)
            {
                var vacancySkills = responce.Content.ReadAsAsync<IEnumerable<VacancySkill>>().Result;

                foreach (var skills in vacancySkills)
                {
                    client.Dispose();
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/" + "skills/" + skills.IdSkill.ToString());
                    responce = client.GetAsync("").Result;
                    if (responce.IsSuccessStatusCode)
                    {
                        var skill = responce.Content.ReadAsAsync<Skill>().Result;
                        Key_skills=Key_skills.Append(skill).ToList();
                    }
                }
            }
            client.Dispose();
            #endregion
            #region specializations
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/" + "vspecs/vacancy/" + Id.ToString());
            responce = client.GetAsync("").Result;
            if (responce.IsSuccessStatusCode)
            {
                var vacancySpecs = responce.Content.ReadAsAsync<IEnumerable<VacancySpecialization>>().Result;

                foreach (var specs in vacancySpecs)
                {
                    client.Dispose();
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/" + "specializations/" + specs.IdSpecialization.ToString());
                    responce = client.GetAsync("").Result;
                    if (responce.IsSuccessStatusCode)
                    {
                        var sp = responce.Content.ReadAsAsync<Specialization>().Result;
                        specializations=specializations.Append(sp).ToList();
                    }
                }
            }
            client.Dispose();
            #endregion
            #region area
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/" + "areas/" + @short.IdArea);
            responce = client.GetAsync("").Result;
            if (responce.IsSuccessStatusCode)
                Area = responce.Content.ReadAsAsync<Area>().Result;
            client.Dispose();
            #endregion
            #region Experience
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/" + "experiences/" + @short.IdExperience);
            responce = client.GetAsync("").Result;
            if (responce.IsSuccessStatusCode)
                Experience = responce.Content.ReadAsAsync<Experience>().Result;
            client.Dispose();
            #endregion
        }
    }
}
