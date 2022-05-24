using HeadhunterGetterClient.Infrastructure.Commands;
using HeadhunterGetterClient.Models;
using HeadhunterGetterClient.ViewModels.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HeadhunterGetterClient.ViewModels
{
    class MainWindowViewModel:ViewModel
    {

        readonly string URL = "https://localhost:44309/api";

        HttpClient client;

        #region Getter
        public string OutPut { get; set; }
        public string VacancyName { get; set; }
        public decimal Salary { get; set; }
        public int VacanciesFound { get; set; }

        public ICommand GetCommand { get; }
        private bool CanGetCommnadExecute(object p) => true;
        private void OnGetCommandExecuted(object p)
        {
            OutPut = Getter.Get(out int i, VacancyName, Salary);
            VacanciesFound = i;
            OnPropertyChanged("OutPut");
            OnPropertyChanged("VacanciesFound");
        }
        #endregion

        #region Vacancies
        public int SearchWidth { get => AreaWidth - ButtonWidth; }
        public int ButtonWidth { get; } = 50;

        public int FullWidth { get; set; } = 800;

        public int AreaWidth { get => FullWidth - 20; }

        public List<ShortVacancy> Vacancies { get; set; } = new List<ShortVacancy>();

        public bool isVacansies { get; set; }
        public bool isVacansy { get; set; }
        public FullVacancy ChoosenVacancy { get; set; }

        public Skill SelectedSkill { get; set; }
        public Skill ToAddSkill { get; set; } = new Skill();
        public Specialization SelectedSpecialization { get; set; }
        public Specialization ToAddSpecialization { get; set; } = new Specialization();
        public string Message { get; set; }

        public ICommand LoadCommand { get; }
        private bool CanLoadCommnadExecute(object p) => true;
        private void OnLoadCommandExecuted(object p)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/vacancies");
            var responce = client.GetAsync("").Result;
            if (responce.IsSuccessStatusCode)
            {
                Vacancies = (List<ShortVacancy>)responce.Content.ReadAsAsync<IEnumerable<ShortVacancy>>().Result;
                OnPropertyChanged("Vacancies");
            }
            client.Dispose();
        }

        public ICommand WatchCommand { get; }
        private bool CanWatchCommnadExecute(object p) => true;
        private void OnWatchCommandExecuted(object p)
        {
            ChoosenVacancy = new FullVacancy(Vacancies.Where(x => x.Id.ToString() == p.ToString()).First());
            isVacansy = true;
            OnPropertyChanged("ChoosenVacancy");
            OnPropertyChanged("isVacansy");
        }

        public ICommand DeleteCommand { get; }
        private bool CanDeleteCommnadExecute(object p) => true;
        private void OnDeleteCommandExecuted(object p)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/vacancies/"+p.ToString());
            var responce = client.DeleteAsync("").Result;
            if (responce.IsSuccessStatusCode)
                Vacancies = Vacancies.Where(x => x.Id.ToString() != p.ToString()).ToList();
            client.Dispose();
            OnPropertyChanged("Vacancies");
        }

        public ICommand UpdateCommand { get; }
        private bool CanUpdateCommnadExecute(object p) => true;
        private void OnUpdateCommandExecuted(object p)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/vacancies/" + p.ToString());
            var responce = client.DeleteAsync("").Result;
            if (responce.IsSuccessStatusCode)
                Vacancies = Vacancies.Where(x => x.Id.ToString() != p.ToString()).ToList();
            client.Dispose();
            OnPropertyChanged("Vacancies");
        }
        public ICommand PostCommand { get; }
        private bool CanPostCommnadExecute(object p) => true;
        private void OnPostCommandExecuted(object p)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/vacancies");
            var vac = new ShortVacancy(ChoosenVacancy);
            var stringContent = new StringContent(JsonConvert.SerializeObject(vac), Encoding.UTF8, "application/json");
            var response = client.PostAsync("", stringContent).Result;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Message = response.ReasonPhrase;
                OnPropertyChanged("Message");
                return;
            }
            client.Dispose();

            foreach (var skill in ChoosenVacancy.Key_skills)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/skills/name/"+skill.Name);
                response = client.GetAsync("").Result;
                client.Dispose();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/skills");
                    stringContent = new StringContent(JsonConvert.SerializeObject(skill), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<Skill> sk = new List<Skill>();
                    try
                    {
                        sk = response.Content.ReadAsAsync<IEnumerable<Skill>>().Result;
                    }
                    catch (Exception e)
                    {
                        string s = response.Content.ReadAsStringAsync().Result;
                        Skill s1 = JsonConvert.DeserializeObject<Skill>(s);
                        sk = sk.Append(s1).ToList();
                    }
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/vskills");
                    stringContent = new StringContent(JsonConvert.SerializeObject(new VacancySkill() {IdSkill=sk.ElementAt(0).Id , IdVacancy=ChoosenVacancy.Id}), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
            }
            client.Dispose();

            foreach (var spec in ChoosenVacancy.specializations)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/specializations/name/" + spec.Name);
                response = client.GetAsync("").Result;
                client.Dispose();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/specialization");
                    stringContent = new StringContent(JsonConvert.SerializeObject(spec), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<Specialization> sk = new List<Specialization>();
                    try
                    {
                        sk = response.Content.ReadAsAsync<IEnumerable<Specialization>>().Result;
                    }
                    catch (Exception e)
                    {
                        string s = response.Content.ReadAsStringAsync().Result;
                        Specialization s1 = JsonConvert.DeserializeObject<Specialization>(s);
                        sk = sk.Append(s1).ToList();
                    }
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/vspecs");
                    stringContent = new StringContent(JsonConvert.SerializeObject(new VacancySpecialization() { IdSpecialization = sk.ElementAt(0).Id, IdVacancy = ChoosenVacancy.Id }), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
            }
            client.Dispose();

            
        }

        public ICommand PutCommand { get; }
        private bool CanPutCommnadExecute(object p) => true;
        private void OnPutCommandExecuted(object p)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
            client.BaseAddress = new Uri(URL + "/vacancies/"+ChoosenVacancy.Id);
            var vac = new ShortVacancy(ChoosenVacancy);
            var stringContent = new StringContent(JsonConvert.SerializeObject(vac), Encoding.UTF8, "application/json");
            var response = client.PutAsync("", stringContent).Result;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Message = response.ReasonPhrase;
                OnPropertyChanged("Message");
                return;
            }
            client.Dispose();

            foreach (var skill in ChoosenVacancy.Key_skills)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/skills/name/" + skill.Name);
                response = client.GetAsync("").Result;
                client.Dispose();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/skills");
                    stringContent = new StringContent(JsonConvert.SerializeObject(skill), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<Skill> sk = new List<Skill>();
                    try
                    {
                        sk = response.Content.ReadAsAsync<IEnumerable<Skill>>().Result;
                    }
                    catch (Exception e)
                    {
                        string s = response.Content.ReadAsStringAsync().Result;
                        Skill s1 = JsonConvert.DeserializeObject<Skill>(s);
                        sk = sk.Append(s1).ToList();
                    }
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/vskills");
                    stringContent = new StringContent(JsonConvert.SerializeObject(new VacancySkill() { IdSkill = sk.ElementAt(0).Id, IdVacancy = ChoosenVacancy.Id }), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
            }
            client.Dispose();

            foreach (var spec in ChoosenVacancy.specializations)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/specializations/name/" + spec.Name);
                response = client.GetAsync("").Result;
                client.Dispose();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/specialization");
                    stringContent = new StringContent(JsonConvert.SerializeObject(spec), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
                if (response.IsSuccessStatusCode)
                {
                    IEnumerable<Specialization> sk = new List<Specialization>();
                    try
                    {
                        sk = response.Content.ReadAsAsync<IEnumerable<Specialization>>().Result;
                    }
                    catch (Exception e)
                    {
                        string s = response.Content.ReadAsStringAsync().Result;
                        Specialization s1 = JsonConvert.DeserializeObject<Specialization>(s);
                        sk = sk.Append(s1).ToList();
                    }
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                    client.BaseAddress = new Uri(URL + "/vspecs");
                    stringContent = new StringContent(JsonConvert.SerializeObject(new VacancySpecialization() { IdSpecialization = sk.ElementAt(0).Id, IdVacancy = ChoosenVacancy.Id }), Encoding.UTF8, "application/json");
                    response = client.PostAsync("", stringContent).Result;
                    client.Dispose();
                }
            }
            client.Dispose();


        }
        #endregion

        public ICommand AddSkillCommand { get; }
        private bool CanAddSkillCommnadExecute(object p) => ToAddSkill.Name!= "" && ToAddSkill.Name != null;
        private void OnAddSkillCommandExecuted(object p)
        {
            ChoosenVacancy.Key_skills=ChoosenVacancy.Key_skills.Append(ToAddSkill);
            OnPropertyChanged("ChoosenVacancy");
            ToAddSkill = new Skill();
            OnPropertyChanged("ToAddSkill");
        }

        public ICommand DeleteSkillCommand { get; }
        private bool CanDeleteSkillCommnadExecute(object p) => SelectedSkill!=null;
        private void OnDeleteSkillCommandExecuted(object p)
        {
            if (SelectedSkill.Id != -1)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/vskills/" + ChoosenVacancy.Id + "/" + SelectedSkill.Id);
                var response = client.DeleteAsync("").Result;
                client.Dispose();
            }
            var a = ChoosenVacancy.Key_skills.ToList();
            a.Remove(SelectedSkill);
            ChoosenVacancy.Key_skills = a;
            SelectedSkill = null;
            OnPropertyChanged("ChoosenVacancy");
            OnPropertyChanged("SelectedSkill");
        }

        public ICommand AddSpecializationCommand { get; }
        private bool CanAddSpecializationCommnadExecute(object p) => ToAddSpecialization.Name != "" && ToAddSpecialization.Name!=null;
        private void OnAddSpecializationCommandExecuted(object p)
        {
            ChoosenVacancy.specializations = ChoosenVacancy.specializations.Append(ToAddSpecialization);
            OnPropertyChanged("ChoosenVacancy");
            ToAddSpecialization = new Specialization();
            OnPropertyChanged("ToAddSpecialization");
        }

        public ICommand DeleteSpecializationCommand { get; }
        private bool CanDeleteSpecializationCommnadExecute(object p) => SelectedSpecialization != null;
        private void OnDeleteSpecializationCommandExecuted(object p)
        {
            if (SelectedSpecialization.Id != "" || SelectedSpecialization.Id!=null)
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Test")));
                client.BaseAddress = new Uri(URL + "/vspecs/" + ChoosenVacancy.Id + "/" + SelectedSpecialization.Id);
                var response = client.DeleteAsync("").Result;
                client.Dispose();
            }
            var a = ChoosenVacancy.specializations.ToList();
            a.Remove(SelectedSpecialization);
            ChoosenVacancy.specializations = a;
            SelectedSpecialization = null;
            OnPropertyChanged("ChoosenVacancy");
            OnPropertyChanged("SelectedSpecialization");
        }
        public MainWindowViewModel()
        {
            GetCommand = new LambdaCommand(OnGetCommandExecuted, CanGetCommnadExecute);
            LoadCommand = new LambdaCommand(OnLoadCommandExecuted, CanLoadCommnadExecute);
            WatchCommand = new LambdaCommand(OnWatchCommandExecuted, CanWatchCommnadExecute);
            DeleteCommand = new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommnadExecute);
            PostCommand = new LambdaCommand(OnPostCommandExecuted, CanPostCommnadExecute);
            PutCommand = new LambdaCommand(OnPutCommandExecuted, CanPutCommnadExecute);
            AddSkillCommand = new LambdaCommand(OnAddSkillCommandExecuted, CanAddSkillCommnadExecute);
            DeleteSkillCommand = new LambdaCommand(OnDeleteSkillCommandExecuted, CanDeleteSkillCommnadExecute);
            AddSpecializationCommand = new LambdaCommand(OnAddSpecializationCommandExecuted, CanAddSpecializationCommnadExecute);
            DeleteSpecializationCommand = new LambdaCommand(OnDeleteSpecializationCommandExecuted, CanDeleteSpecializationCommnadExecute);
        }
    }
}
