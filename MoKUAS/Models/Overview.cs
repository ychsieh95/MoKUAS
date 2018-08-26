using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class Overview
    {
        public Class ClassInfo { get; set; }

        public decimal CommentCount { get; set; }

        public TeachingMethods TeachingMethods { get; set; }

        public RollCallMethods RollCallMethods { get; set; }

        public ClassworkExam ClassworkExam { get; set; }

        public Grades Grades { get; set; }
    }

    public class TeachingMethods
    {
        public decimal IsBlackboard { get; set; }
        public decimal IsBlackboardPercent { get; set; }

        public decimal IsBook { get; set; }
        public decimal IsBookPercent { get; set; }

        public decimal IsPPT { get; set; }
        public decimal IsPPTPercent { get; set; }

        public decimal IsBroadcast { get; set; }
        public decimal IsBroadcastPercent { get; set; }

        public decimal IsBuild { get; set; }
        public decimal IsBuildPercent { get; set; }

        public decimal IsInteractive { get; set; }
        public decimal IsInteractivePercent { get; set; }
    }

    public class RollCallMethods
    {
        public decimal RollCallFrequency { get; set; }
        private decimal rollCallFrequencyPercent = 0;
        public decimal RollCallFrequencyPercent
        {
            get
            {
                return rollCallFrequencyPercent;
            }
            set
            {
                rollCallFrequencyPercent = value;

                if (value <= 0)
                {
                    RollCallFrequencyStr = "無";
                }
                else if (0 < value && value < 25)
                {
                    RollCallFrequencyStr = "很低";
                }
                else if (value == 25)
                {
                    RollCallFrequencyStr = "低";
                }
                else if (25 < value && value < 50)
                {
                    RollCallFrequencyStr = "偏低";
                }
                else if (value == 50)
                {
                    RollCallFrequencyStr = "中";
                }
                else if (50 < value && value < 75)
                {
                    RollCallFrequencyStr = "偏高";
                }
                else if (value == 75)
                {
                    RollCallFrequencyStr = "高";
                }
                else if (75 < value && value < 100)
                {
                    RollCallFrequencyStr = "很高";
                }
                else if (value >= 100)
                {
                    RollCallFrequencyStr = "每堂";
                }
            }
        }
        public string RollCallFrequencyStr { get; set; } = "無";

        public decimal ByInPerson { get; set; }
        public decimal ByInPersonPercent { get; set; }

        public decimal BySignInSheet { get; set; }
        public decimal BySignInSheetPercent { get; set; }

        public decimal ByOnline { get; set; }
        public decimal ByOnlinePercent { get; set; }

        public decimal ByClasswork { get; set; }
        public decimal ByClassworkPercent { get; set; }

        public decimal ByTest { get; set; }
        public decimal ByTestPercent { get; set; }
    }

    public class ClassworkExam
    {
        public decimal HaveClasswork { get; set; }
        public decimal HaveClassworkPercent { get; set; }

        public decimal HaveTest { get; set; }
        public decimal HaveTestPercent { get; set; }

        public decimal HaveMidtermExam { get; set; }
        public decimal HaveMidtermExamPercent { get; set; }

        public decimal HaveFinalExam { get; set; }
        public decimal HaveFinalExamPercent { get; set; }
    }

    public class Grades
    {
        public decimal Grade1 { get; set; }
        public decimal Grade1Percent { get; set; }
        public decimal Grade2 { get; set; }
        public decimal Grade2Percent { get; set; }
        public decimal Grade3 { get; set; }
        public decimal Grade3Percent { get; set; }
        public decimal Grade4 { get; set; }
        public decimal Grade4Percent { get; set; }
        public decimal Grade5 { get; set; }
        public decimal Grade5Percent { get; set; }

        public decimal GradeAverage { get; set; }
    }
}