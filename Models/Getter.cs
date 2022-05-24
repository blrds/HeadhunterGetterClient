using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HeadhunterGetterClient.Models
{
    public class Area
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Specialization
    {
        public string Id { get; set; } = "";
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class Experience
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class Salary
    {
        public string From { get; set; } = "0";
        public string To { get; set; } = "0";
        public string Currency { get; set; } = "RUR";
    }

    public class Snippet
    {
        public string Requirement { get; set; }

        public string Responsibility { get; set; }
    }
    public class Skill
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class DataObject
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
    }

    public class Ids
    {
        public int Id { get; set; }
    }

    public class Collection
    {
        public IEnumerable<Ids> Items { get; set; }
    }
    class Getter
    {
        private const string URL = "https://api.hh.ru/vacancies";
        public static string Get(out int count, string name = "", decimal salary = -1)
        {
            count = 0;
            string answer = "";
            string urlParametrs = "";
            if (name != "" || salary != -1) urlParametrs = "&";
            if (name != "")
            {
                urlParametrs += "text=\"" + name + "\"";
            }
            if (name != "" && salary != -1) urlParametrs += "&";
            if (salary != -1) urlParametrs += "salary=" + salary.ToString("0");
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            HttpResponseMessage response = client.GetAsync("").Result;
            Collection ids = null;
            if (response.IsSuccessStatusCode)
            {
                ids = response.Content.ReadAsAsync<Collection>().Result;
                count = ids.Items.Count();
            }
            client.Dispose();
            foreach (var id in ids.Items)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(URL + "/" + id.Id.ToString());
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));

                response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    var dataObject = response.Content.ReadAsAsync<DataObject>().Result;
                    answer += response.Content.ReadAsStringAsync().Result + "\n";
                    MySqlConnection conn = new MySqlConnection(@"server=127.0.0.1;uid=root;port=3306;pwd=12345;database=headhunter");
                    conn.Open();
                    string command = "SELECT count(*) from areas where id=" + dataObject.Area.Id.ToString();
                    MySqlCommand com = new MySqlCommand(command, conn);
                    var a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO areas (id, name) values(" + dataObject.Area.Id + ",'" + dataObject.Area.Name + "')";
                    }
                    else
                    {
                        com.CommandText = "update areas set name='" + dataObject.Area.Name + "' where id=" + dataObject.Area.Id;
                    }
                    com.ExecuteNonQuery();
                    if (dataObject.specializations != null && dataObject.specializations.Count() != 0)
                        foreach (var spec in dataObject.specializations)
                        {
                            com.CommandText = "SELECT count(*) from specializations where id=" + spec.Id;
                            a = com.ExecuteScalar().ToString();
                            if (int.Parse(a) == 0)
                            {
                                com.CommandText = "INSERT INTO specializations (id, name) values(" + spec.Id + ",'" + spec.Name + "')";
                            }
                            else
                            {
                                com.CommandText = "update specializations set name='" + spec.Name + "' where id=" + spec.Id;
                            }
                            com.ExecuteNonQuery();
                        }
                    if (dataObject.Key_skills != null && dataObject.Key_skills.Count() != 0)
                        foreach (var skill in dataObject.Key_skills)
                        {
                            com.CommandText = "SELECT count(*) from skills where name='" + skill.Name+"'";
                            a = com.ExecuteScalar().ToString();
                            if (int.Parse(a) == 0)
                            {
                                com.CommandText = "INSERT INTO skills (name) values('" + skill.Name + "')";
                                com.ExecuteNonQuery();
                            }
                        }
                    com.CommandText = "SELECT count(*) from experiences where id='" + dataObject.Experience.Id.ToString()+"'";
                    a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO experiences (id, name) values('" + dataObject.Experience.Id + "','" + dataObject.Experience.Name + "')";
                    }
                    else
                    {
                        com.CommandText = "update experiences set name='" + dataObject.Experience.Name + "' where id='" + dataObject.Experience.Id+"'";
                    }
                    com.ExecuteNonQuery();
                    com.CommandText = "SELECT count(*) from vacancies where id=" + dataObject.Id.ToString();
                    a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO " +
                            "vacancies (id, name, idarea, salaryfrom, salaryto, salarycurrency, publisheddate, snippetrequirement, snippetresponsibility, description, idexperience)" +
                            "values(" + dataObject.Id + ",'" + dataObject.Name + "'," + dataObject.Area.Id + "," + (dataObject.Salary?.From == null ? "0" : dataObject.Salary.From) + "," + (dataObject.Salary?.To == null ? "0" : dataObject.Salary.To) + ",'"
                             + (dataObject.Salary?.Currency == null ? "RUR" : dataObject.Salary.Currency) + "','" + dataObject.Published_at.ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + dataObject.Snippet?.Requirement + "','" + dataObject.Snippet?.Responsibility + "','" + dataObject.Description + "','" + dataObject.Experience.Id + "')";
                    }
                    else
                    {
                        com.CommandText = "update vacancies " +
                            "set name='" + dataObject.Name + "', " +
                            "idarea=" + dataObject.Area.Id + ", " +
                            "salaryfrom=" + (dataObject.Salary?.From == null ? "0" : dataObject.Salary.From) +
                            ", salaryto=" + (dataObject.Salary?.To==null?"0":dataObject.Salary.To) + ", " +
                            "salarycurrency='" + (dataObject.Salary?.Currency == null ? "RUR" : dataObject.Salary.Currency) +"', " +
                            "publisheddate='" + dataObject.Published_at.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', " +
                            "snippetrequirement='" + dataObject.Snippet?.Requirement +
                            "', snippetresponsibility='" + dataObject.Snippet?.Responsibility + "', " +
                            "description='" + dataObject?.Description + "', " +
                            "idexperience='" + dataObject.Experience.Id + "' " +
                            "where id=" + dataObject.Id;
                    }
                    com.ExecuteNonQuery();
                    if (dataObject.specializations != null && dataObject.specializations.Count() != 0)
                    {
                        com.CommandText = "delete from vacancyspecializations where idvacancy=" + dataObject.Id.ToString();
                        com.ExecuteNonQuery();
                        foreach (var spec in dataObject.specializations)
                        {
                            com.CommandText = "INSERT INTO vacancyspecializations (idvacancy, idspecialization) values(" + dataObject.Id + "," + spec.Id + ")";
                            com.ExecuteNonQuery();
                        }
                    }
                    if (dataObject.Key_skills != null && dataObject.Key_skills.Count() != 0)
                    {
                        com.CommandText = "delete from vacancyskills where idvacancy=" + dataObject.Id.ToString();
                        com.ExecuteNonQuery();
                        foreach (var skill in dataObject.Key_skills)
                        {
                            com.CommandText = "SELECT id from skills where name='" + skill.Name+"'";
                            a = com.ExecuteScalar().ToString();
                            com.CommandText = "INSERT INTO vacancyskills (idvacancy, idskill) values(" + dataObject.Id + "," + int.Parse(a) + ")";

                            com.ExecuteNonQuery();
                        }
                    }
                }
                client.Dispose();
            }
            return answer;
        }

    }
}
