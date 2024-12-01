using StudentManagement.Models;
using StudentManagement.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagement.Services;
using System.Collections.ObjectModel;

namespace StudentManagement.Objects
{
    public class SemesterDataGrid : BaseObjectWithBaseViewModel, IBaseCard
    {
     
        private string _displayName;
        private Guid _idSemester;
        private string _batch;
       
        public string Batch
        {
            get => _batch;
            set => _batch = value;
        }
        public Guid IdSemester
        {
            get => _idSemester;
            set => _idSemester = value;
        }
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;
        }
       

       

        public SemesterDataGrid(Guid IdSemester, string DisplayName, string Batch)
        {
            this.IdSemester = IdSemester;
            this.Batch = Batch;
            this.DisplayName = DisplayName;
       
         

        }

    }
}
