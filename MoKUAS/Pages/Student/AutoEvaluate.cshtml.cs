using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoKUAS.Pages.Student
{
    public class AutoEvaluateModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Models.Evaluation Evaluation { get; set; }

        public List<SelectListItem> EvaliationTypes { get; private set; }
        public List<SelectListItem> EvaliationSubjects { get; private set; }
        public List<SelectListItem> FavoriteRanks { get; private set; }
        public List<SelectListItem> Genders { get; private set; }
        public List<SelectListItem> Colleges { get; private set; }
        public List<SelectListItem> EductionalSystems { get; private set; }
        public List<SelectListItem> Grades { get; private set; }
        public List<Models.EvaluationItem> EvaluationItems { get; private set; }

        public void OnGet()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();
            var evaluation = new Models.Evaluation();

            EvaluationItems = kuasAp.ListEvaluationForm(username: student.Username, password: student.Password);
            if (EvaluationItems == null)
            {
                ModelState.AddModelError("Error", "教學評量尚未開放填寫");
                EvaluationItems = new List<Models.EvaluationItem>();
            }

            // Evaliation types
            EvaliationTypes = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.EvaliationType))
                .OfType<Models.EvaliationType>()
                .ToList()
                .ForEach(evaliationType => EvaliationTypes.Add(new SelectListItem()
                {
                    Text = evaliationType.ToString(),
                    Value = ((int)evaliationType).ToString()
                }));
            EvaliationTypes.Find(evaliationType => evaliationType.Text.Equals((
                ((3 <= DateTime.Now.Month && DateTime.Now.Month <= 5) || (9 <= DateTime.Now.Month && DateTime.Now.Month <= 12)) ?
                Models.EvaliationType.期中評量 : Models.EvaliationType.期末評量).ToString())).Selected = true;

            // Evaliation subjects
            EvaliationSubjects = new List<SelectListItem>();
            if (EvaluationItems.Count > 0)
            {
                EvaluationItems
                    .FindAll(evaluationItem => !evaluationItem.Done)
                    .ForEach(evaluationItem =>
                        EvaliationSubjects.Add(new SelectListItem()
                        {
                            Text = $"{evaluationItem.SubjectChineseName}（{evaluationItem.Teachers}）",
                            Value = evaluationItem.Title
                        }));
                EvaliationSubjects.First().Selected = true;
            }

            // Favorite ranks
            FavoriteRanks = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.FavoriteRank))
                .OfType<Models.FavoriteRank>()
                .ToList()
                .ForEach(favoriteRank =>
                    FavoriteRanks.Add(new SelectListItem()
                    {
                        Text = favoriteRank.ToString(),
                        Value = ((int)favoriteRank).ToString()
                    }));
            FavoriteRanks.Find(favoriteRank => favoriteRank.Text == evaluation.FavoriteRank.ToString()).Selected = true;

            // Genders
            Genders = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.Gender))
                .OfType<Models.Gender>()
                .ToList()
                .ForEach(gender =>
                    Genders.Add(new SelectListItem()
                    {
                        Text = gender.ToString(),
                        Value = ((int)gender).ToString()
                    }));
            Genders.Find(gender => gender.Text == evaluation.Gender.ToString()).Selected = true;

            // Colleges
            Colleges = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.College))
                .OfType<Models.College>()
                .ToList()
                .ForEach(college =>
                    Colleges.Add(new SelectListItem()
                    {
                        Text = college.ToString(),
                        Value = ((int)college).ToString()
                    }));
            Colleges.Find(college => college.Text == evaluation.College.ToString()).Selected = true;

            // Eductional Systems
            EductionalSystems = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.EductionalSystem))
                .OfType<Models.EductionalSystem>()
                .ToList()
                .ForEach(eductionalSystem =>
                    EductionalSystems.Add(new SelectListItem()
                    {
                        Text = eductionalSystem.ToString(),
                        Value = ((int)eductionalSystem).ToString()
                    }));
            EductionalSystems.Find(eductionalSystem => eductionalSystem.Text == evaluation.EductionalSystem.ToString()).Selected = true;

            // Grades
            Grades = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.Grade))
                .OfType<Models.Grade>()
                .ToList()
                .ForEach(grade =>
                    Grades.Add(new SelectListItem()
                    {
                        Text = grade.ToString(),
                        Value = ((int)grade).ToString()
                    }));
            Grades.Find(grade => grade.Text == evaluation.Grade.ToString()).Selected = true;
        }

        public void OnPost()
        {
            var student = JsonConvert.DeserializeObject<Models.Student>(
                User.Claims.First(claim => claim.Type == "Information").Value);
            var kuasAp = new Services.KUASAPService();

            // Fill in evaliation form
            var doneList = new List<Models.EvaluationItem>();
            switch (Evaluation.EvaliationType)
            {
                case Models.EvaliationType.期中評量:
                    doneList = kuasAp.FillInMidtermEvaluationForm(
                        username: student.Username,
                        password: student.Password,
                        evaluation: Evaluation);
                    break;
                case Models.EvaliationType.期末評量:
                    doneList = kuasAp.FillInFinalEvaluationForm(
                        username: student.Username,
                        password: student.Password,
                        evaluation: Evaluation);
                    break;
            }

            // List evaliation item
            EvaluationItems = kuasAp.ListEvaluationForm(username: student.Username, password: student.Password);
            if (EvaluationItems == null)
            {
                ModelState.AddModelError("Error", "教學評量尚未開放填寫");
                EvaluationItems = new List<Models.EvaluationItem>();
            }

            if (doneList == null)
                ModelState.AddModelError("Error", "教學評量尚未開放填寫");
            else if (doneList.Count == 0)
                ModelState.AddModelError("Error", "未填寫任何教學評量");
            else if (Evaluation.EvaliationType == Models.EvaliationType.期中評量)
                ModelState.AddModelError("Error", $"已自動填寫 { doneList.Count(x => x.EvaliationType == Models.EvaliationType.期中評量) } 個期中教學評量");
            else if (Evaluation.EvaliationType == Models.EvaliationType.期末評量)
                ModelState.AddModelError("Error", $"已自動填寫 { doneList.Count(x => x.EvaliationType == Models.EvaliationType.期末評量) } 個期末教學評量");

            // Evaliation types
            EvaliationTypes = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.EvaliationType))
                .OfType<Models.EvaliationType>()
                .ToList()
                .ForEach(evaliationType => EvaliationTypes.Add(new SelectListItem()
                {
                    Text = evaliationType.ToString(),
                    Value = ((int)evaliationType).ToString()
                }));
            EvaliationTypes.Find(evaliationType => evaliationType.Text == Evaluation.EvaliationType.ToString()).Selected = true;

            // Evaliation subjects
            EvaliationSubjects = new List<SelectListItem>();
            EvaluationItems
                .FindAll(evaluationItem => !evaluationItem.Done)
                .ForEach(evaluationItem =>
                    EvaliationSubjects.Add(new SelectListItem()
                    {
                        Text = $"{ evaluationItem.SubjectChineseName }（ {evaluationItem.Teachers }）",
                        Value = evaluationItem.Title
                    }));
            EvaliationSubjects.Insert(0, new SelectListItem() { Text = $"所有評量 ({ EvaluationItems.Count(x => !x.Done) })", Value = "AllEvaluation" });
            EvaliationSubjects.First().Selected = true;

            // Favorite ranks
            FavoriteRanks = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.FavoriteRank))
                .OfType<Models.FavoriteRank>()
                .ToList()
                .ForEach(favoriteRank =>
                    FavoriteRanks.Add(new SelectListItem()
                    {
                        Text = favoriteRank.ToString(),
                        Value = ((int)favoriteRank).ToString()
                    }));
            FavoriteRanks.Find(favoriteRank => favoriteRank.Text == Evaluation.FavoriteRank.ToString()).Selected = true;

            // Genders
            Genders = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.Gender))
                .OfType<Models.Gender>()
                .ToList()
                .ForEach(gender =>
                    Genders.Add(new SelectListItem()
                    {
                        Text = gender.ToString(),
                        Value = ((int)gender).ToString()
                    }));
            Genders.Find(gender => gender.Text == Evaluation.Gender.ToString()).Selected = true;

            // Colleges
            Colleges = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.College))
                .OfType<Models.College>()
                .ToList()
                .ForEach(college =>
                    Colleges.Add(new SelectListItem()
                    {
                        Text = college.ToString(),
                        Value = ((int)college).ToString()
                    }));
            Colleges.Find(college => college.Text == Evaluation.College.ToString()).Selected = true;

            // Eductional Systems
            EductionalSystems = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.EductionalSystem))
                .OfType<Models.EductionalSystem>()
                .ToList()
                .ForEach(eductionalSystem =>
                    EductionalSystems.Add(new SelectListItem()
                    {
                        Text = eductionalSystem.ToString(),
                        Value = ((int)eductionalSystem).ToString()
                    }));
            EductionalSystems.Find(eductionalSystem => eductionalSystem.Text == Evaluation.EductionalSystem.ToString()).Selected = true;

            // Grades
            Grades = new List<SelectListItem>();
            Enum.GetValues(typeof(Models.Grade))
                .OfType<Models.Grade>()
                .ToList()
                .ForEach(grade =>
                    Grades.Add(new SelectListItem()
                    {
                        Text = grade.ToString(),
                        Value = ((int)grade).ToString()
                    }));
            Grades.Find(grade => grade.Text == Evaluation.Grade.ToString()).Selected = true;
        }
    }
}