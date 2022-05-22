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
    }

    public class Specialization
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Experience
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Salary
    {
        public string From { get; set; } = "0";
        public string To { get; set; } = "0";
    }

    public class Snippet
    {
        public string Requirement { get; set; }

        public string Responsibility { get; set; }
    }
    public class Skill
    {
        public string Name { get; set; }
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

        public string Discription { get; set; }
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
        public static void Get(string name = "", decimal salary = -1)
        {
            string urlParametrs = "?";
            if (name != "")
            {
                urlParametrs += "text=\"" + name + "\"";
            }
            if (name != "" && salary != -1) urlParametrs += "&";
            if (salary != -1) urlParametrs += "salary=" + salary;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            HttpResponseMessage response = client.GetAsync("").Result;
            Collection ids = null;
            if (response.IsSuccessStatusCode)
            {
                ids = response.Content.ReadAsAsync<Collection>().Result;

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
                    
                    MySqlConnection conn = new MySqlConnection(@"server=127.0.0.1;uid=root;port=3306;pwd=12345;database=headhunter");
                    conn.Open();
                    string command = "SELECT count(*) from area where id=" + dataObject.Area.Id.ToString();
                    MySqlCommand com = new MySqlCommand(command, conn);
                    var a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO area (id, name) values(" + dataObject.Area.Id + ",'" + dataObject.Area.Name + "')";
                    }
                    else
                    {
                        com.CommandText = "update area set name='" + dataObject.Area.Name + "' where id=" + dataObject.Area.Id;
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
                            }
                            com.ExecuteNonQuery();
                        }
                    com.CommandText = "SELECT count(*) from experience where id='" + dataObject.Experience.Id.ToString()+"'";
                    a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO experience (id, name) values('" + dataObject.Experience.Id + "','" + dataObject.Experience.Name + "')";
                    }
                    else
                    {
                        com.CommandText = "update experience set name='" + dataObject.Experience.Name + "' where id='" + dataObject.Experience.Id+"'";
                    }
                    com.ExecuteNonQuery();
                    com.CommandText = "SELECT count(*) from vacancies where id=" + dataObject.Id.ToString();
                    a = com.ExecuteScalar().ToString();
                    if (int.Parse(a) == 0)
                    {
                        com.CommandText = "INSERT INTO " +
                            "vacancies (id, name, idarea, salaryfrom, salaryto, publishedate, snippetrequirement, snippetresponsibility, description, idexperience)" +
                            "values(" + dataObject.Id + ",'" + dataObject.Name + "'," + dataObject.Area.Id + "," + (dataObject.Salary?.From == null ? "0" : dataObject.Salary.From) + "," + (dataObject.Salary?.To == null ? "0" : dataObject.Salary.To) + ",'"
                            + dataObject.Published_at.ToString("yyyy-MM-dd HH:mm:ss.fff") + "','" + dataObject.Snippet?.Requirement + "','" + dataObject.Snippet?.Responsibility + "','" + dataObject.Discription + "','" + dataObject.Experience.Id + "')";
                    }
                    else
                    {
                        com.CommandText = "update vacancies " +
                            "set name='" + dataObject.Name + "', " +
                            "idarea=" + dataObject.Area.Id + ", " +
                            "salaryfrom=" + (dataObject.Salary?.From == null ? "0" : dataObject.Salary.From) +
                            ", salaryto=" + (dataObject.Salary?.To==null?"0":dataObject.Salary.To) + ", " +
                            "publishedate='" + dataObject.Published_at.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', " +
                            "snippetrequirement='" + dataObject.Snippet?.Requirement +
                            "', snippetresponsibility='" + dataObject.Snippet?.Responsibility + "', " +
                            "description='" + dataObject?.Discription + "', " +
                            "idexperience='" + dataObject.Experience.Id + "' " +
                            "where id=" + dataObject.Id;
                    }
                    com.ExecuteNonQuery();
                    if (dataObject.specializations != null && dataObject.specializations.Count() != 0)
                    {
                        com.CommandText = "delete from vacanciesspecialization where idvacancy=" + dataObject.Id.ToString();
                        com.ExecuteNonQuery();
                        foreach (var spec in dataObject.specializations)
                        {
                            com.CommandText = "INSERT INTO vacanciesspecialization (idvacancy, idspecialization) values(" + dataObject.Id + "," + spec.Id + ")";
                            com.ExecuteNonQuery();
                        }
                    }
                    if (dataObject.Key_skills != null && dataObject.Key_skills.Count() != 0)
                    {
                        com.CommandText = "delete from vacanciesskills where idvacacncy=" + dataObject.Id.ToString();
                        com.ExecuteNonQuery();
                        foreach (var skill in dataObject.Key_skills)
                        {
                            com.CommandText = "SELECT count(*) from skills where name='" + skill.Name+"'";
                            a = com.ExecuteScalar().ToString();
                            com.CommandText = "INSERT INTO vacanciesskills (idvacacncy, idskill) values(" + dataObject.Id + "," + int.Parse(a) + ")";

                            com.ExecuteNonQuery();
                        }
                    }
                }
                client.Dispose();
            }
        }

    }
}
