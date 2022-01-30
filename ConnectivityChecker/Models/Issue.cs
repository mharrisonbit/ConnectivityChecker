using System;
using SQLite;

namespace ConnectivityChecker.Models
{
    [Table("Issue")]
    public class Issue
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string LostOccurred { get; set; }
    }
}