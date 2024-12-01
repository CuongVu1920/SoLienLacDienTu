using StudentManagement.Commands;
using StudentManagement.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StudentManagement.Objects;
using StudentManagement.Services;
using StudentManagement.Models;
using System.Data.Entity.Infrastructure;

namespace StudentManagement.ViewModels
{
    public class AdminStudentListViewModel : BaseViewModel
    {
        public string SearchQuery { get => _searchQuery; set { _searchQuery = value; SearchNameFunction(); OnPropertyChanged(); } }
        private string _searchQuery;

        private Canvas _mainCanvas;
        public Canvas MainCanvas
        {
            get => _mainCanvas;
            set
            {
                _mainCanvas = value;
            }
        }

        private ObservableCollection<PieChartElement> _dataPieChart;
        public ObservableCollection<PieChartElement> DataPieChart
        {
            get => _dataPieChart;
            set
            {
                _dataPieChart = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<StudentGrid> _bindingData;
        public ObservableCollection<StudentGrid> BindingData
        {
            get => _bindingData;
            set
            {
                _bindingData = value;
                OnPropertyChanged();
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        private bool _isSelectedAll;
        public bool IsSelectedAll
        {
            get => _isSelectedAll;
            set
            {
                _isSelectedAll = value;

                foreach (var student in BindingData)
                {
                    student.IsSelected = value;
                }

                OnPropertyChanged();
            }
        }

       

        private ObservableCollection<StudentGrid> _studentDatabase;
        public ObservableCollection<StudentGrid> StudentDatabase
        {
            get => _studentDatabase;
            set => _studentDatabase = value;
        }

        private ObservableCollection<StudentGrid> _studentClass;
        public ObservableCollection<StudentGrid> StudentClass
        {
            get => _studentClass;
            set => _studentClass = value;
        }

        private ObservableCollection<StudentGrid> _deleteStudentList;
        public ObservableCollection<StudentGrid> DeleteStudentList
        {
            get => _deleteStudentList;
            set => _deleteStudentList = value;
        }

        public ICommand SearchName { get; set; }
        public ICommand DeleteStudent { get; set; }
        public ICommand AddStudent { get; set; }

        private float _pieWidth, _pieHeight, _centerX, _centerY, _radius;

        SubjectClass SubjectClassDetail { get; set; }

        public AdminStudentListViewModel(SubjectClass subjectClass)
        {
            SubjectClassDetail = subjectClass;

            FirstLoadData();

            InitChartParemeter();

            BindingData = new ObservableCollection<StudentGrid>(StudentClass);
            InitCommand();

            SearchNameFunction();
        }

        #region Methods

        public void FirstLoadData()
        {
            try
            {
                // Load list students
                StudentClass = new ObservableCollection<StudentGrid>();
                var students = CourseRegisterServices.Instance.FindStudentsBySubjectClassId(SubjectClassDetail.Id);
                for (int index = 0; index < students.Count; index++)
                {
                    StudentClass.Add(StudentServices.Instance.ConvertStudentToStudentGrid(students[index], index + 1));
                }

               
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể tải thông tin sinh viên", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void InitChartParemeter()
        {
            _pieWidth = 180; _pieHeight = 180;
            _centerX = _pieWidth / 2; _centerY = _pieHeight / 2;
            _radius = _pieWidth / 2;

            DataPieChart = new ObservableCollection<PieChartElement>();

            MainCanvas = new Canvas
            {
                Width = _pieWidth,
                Height = _pieHeight
            };
        }

        private void InitCommand()
        {
            SearchName = new RelayCommand<object>((p) => true, (p) => SearchNameFunction());
            DeleteStudent = new RelayCommand<object>((p) => true, (p) => DeleteStudentFunction());
            AddStudent = new RelayCommand<object>((p) => true, (p) => AddStudentFunction());
        }

        private float RecalculateStudentRating(int number, int totalNumber)
        {
            return (float)number * 100 / totalNumber;
        }

       

        private bool IsStudentExistInClass(string studentId)
        {
            return StudentClass.Any(student => student.Username == studentId);
        }

        private StudentGrid FindSearchQueryInDatabase(string username)
        {
            try
            {
                var user = UserServices.Instance.FindUserByUsername(username);
                return StudentServices.Instance.ConvertStudentToStudentGrid(StudentServices.Instance.GetStudentbyUser(user));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void AddStudentFunction()
        {
            try
            {
                if (IsStudentExistInClass(SearchQuery))
                {
                    MyMessageBox.Show("Sinh viên " + SearchQuery + " đã tồn tại ở lớp học!", "Thêm sinh viên", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StudentGrid findStudent = FindSearchQueryInDatabase(SearchQuery);

                if (findStudent == null)
                {
                    MyMessageBox.Show("Mã số sinh viên " + SearchQuery + " không hợp lệ !", "Thêm sinh viên", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MyMessageBox.Show($"Bạn có muốn thêm sinh viên {findStudent.DisplayName} vào lớp học?", "Thêm sinh viên", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    await CourseRegisterServices.Instance.StudentRegisterSubjectClassDetailToDatabase(findStudent.Id, SubjectClassDetail);

                  //  var componentScore = ScoreServices.Instance.LoadComponentScoreOfSubjectClass(SubjectClassDetail.Id);
                   

                    StudentClass.Add(findStudent);

                    MainWindow.Notify.ShowBalloonTip(3000, SubjectClassDetail.Code, $"Sinh viên {findStudent.DisplayName} đã được thêm vào lớp học!", ToolTipIcon.Info);

                    //MyMessageBox.Show($"Sinh viên {findStudent.DisplayName} đã được thêm vào lớp học!", "Thêm sinh viên", MessageBoxButton.OK, MessageBoxImage.Information);

                    SearchQuery = "";
                }
            }
            catch (DbUpdateException)
            {
                MyMessageBox.Show("Lớp học đã đủ số lượng, thêm sinh viên không thành công");
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Thêm không thành công", "Thêm sinh viên", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       

        
        void SearchNameFunction()
        {
            if (SearchQuery == null)
            {
                SearchQuery = "";
            }

            int stt = 0;
            BindingData.Clear();
            foreach (var student in StudentClass)
            {
                if (VietnameseStringNormalizer.Instance.Normalize(student.DisplayName)
                    .Contains(VietnameseStringNormalizer.Instance.Normalize(SearchQuery))
                    || student.Username.Contains(SearchQuery)
                )
                {
                    student.Number = stt + 1;
                    stt += 1;
                    BindingData.Add(student);
                }
            }

  
        }

        private async void DeleteStudentFunction()
        {
            try
            {
                DeleteStudentList = new ObservableCollection<StudentGrid>();
                foreach (var student in BindingData)
                {
                    if (student.IsSelected)
                    {
                        DeleteStudentList.Add(student);
                    }
                }

                if (DeleteStudentList.Count() == 0)
                {
                    MyMessageBox.Show("Vui lòng chọn sinh viên cần xóa!", "Xóa sinh viên", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBoxResult messageBoxResult = MyMessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa {DeleteStudentList.Count()} sinh viên đã chọn?",
                    "Xóa sinh viên",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    foreach (var student in DeleteStudentList)
                    {
                      //  await ScoreServices.Instance.DeleteStudentScoreByIdAsync(SubjectClassDetail.Id, student.Id);

                        await CourseRegisterServices.Instance.StudentUnregisterSubjectClassDetailToDatabase(student.Id, SubjectClassDetail);

                        StudentClass.Remove(student);
                    }
                    IsSelectedAll = false;
                }
                else return;

                FirstLoadData();
                SearchNameFunction();
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Xóa không thành công", "Xóa sinh viên", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
