using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoKUAS.Models
{
    public class Evaluation
    {
        /// <summary>
        /// 評量類別
        /// </summary>
        public EvaliationType EvaliationType { get; set; } = EvaliationType.期中評量;

        /// <summary>
        /// 填寫之教學評量
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 填寫之教學評量評價
        /// </summary>
        public FavoriteRank FavoriteRank { get; set; } = FavoriteRank.普通;

        /// <summary>
        /// 性別
        /// </summary>
        public Gender Gender { get; set; } = Gender.男性;

        /// <summary>
        /// 學院
        /// </summary>
        public College College { get; set; } = College.電資學院;

        /// <summary>
        /// 學制
        /// </summary>
        public EductionalSystem EductionalSystem { get; set; } = EductionalSystem.日間部四技;

        /// <summary>
        /// 年級
        /// </summary>
        public Grade Grade { get; set; } = Grade.一年級;
    }

    public enum EvaliationType
    {
        期中評量,
        期末評量
    }

    public enum Gender
    {
        男性,
        女性
    }

    public enum College
    {
        電資學院,
        工學院,
        人文社會學院,
        管理學院
    }

    public enum EductionalSystem
    {
        日間部二技,
        日間部四技,
        日間部碩士班,
        日間部博士班,
        進推處二技,
        進推處四技,
        進推處碩專班,
        進推處產碩班,
        進修學院二技
    }

    public enum Grade
    {
        一年級,
        二年級,
        三年級,
        四年級,
        大學延畢
    }

    public enum FavoriteRank
    {
       極優,
       優良,
       普通,
       差勁,
       極差
    }
}