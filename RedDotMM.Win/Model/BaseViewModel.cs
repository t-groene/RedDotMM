using RedDotMM.Win.Data;
using RedDotMM.Win.UIHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RedDotMM.Win.Model
{
    public  abstract class BaseViewModel : INotifyPropertyChanged
    {

        private RedDotMM_Context _context;


        private static int NextIdNo = 0; // Static variable to keep track of the next ID number

        protected int nextID;


        public event EventHandler<EventArgs>? SchliessenEvent;

        public RedDotMM_Context Context
        {
            get
            {
                if (_context == null)
                {
                    _context = new RedDotMM_Context();
                }
                return _context;
            }
            set
            {
                if (_context != value)
                {
                    _context = value;
                    OnPropertyChanged(nameof(Context));
                }
            }
        }


        public BaseViewModel(bool canSave, bool canNew, bool canDelete)
        {
            CanSave = canSave;
            CanNew = canNew;
            CanDelete = canDelete;
            SpeichernCommand = new RelayCommand(Speichern, () => CanSave);
            NeuesElementCommand = new RelayCommand(NeuesElement, () => CanNew);
            LoeschenCommand = new RelayCommand(Loeschen, () => CanDelete);
            SchliessenCommand = new RelayCommand(Schliessen, () => true);
            nextID = NextIdNo++;
        }

       
           

        private bool canSave = false;
        public bool CanSave
        {
            get => canSave;
            set
            {
                if (canSave != value)
                {
                    canSave = value;
                    OnPropertyChanged(nameof(CanSave));     
                    
                }
            }
        }

        private bool _CanNew { get; set; } = false;
        public bool CanNew
        {
            get => _CanNew;
            set
            {
                if (_CanNew != value)
                {
                    _CanNew = value;
                    OnPropertyChanged(nameof(CanNew));                   
                }
            }
        }   


        private bool _CanDelete { get; set; } = false;
        public bool CanDelete
        {
            get => _CanDelete;
            set
            {
                if (_CanDelete != value)
                {
                    _CanDelete = value;
                    OnPropertyChanged(nameof(CanDelete));
                    
                }
            }
        }   



     


        public abstract void NeuesElement();
        public abstract void Loeschen();
        public abstract void Speichern();

        public abstract void Schliessen();


        private ICommand _SpeichernCommand;

        public ICommand SpeichernCommand
        {
            get => _SpeichernCommand;
            set
            {
                if (_SpeichernCommand != value)
                {
                    _SpeichernCommand = value;
                    OnPropertyChanged(nameof(SpeichernCommand));
                }
            }
        }   


        public abstract string AnzeigeName { get;  }


        private ICommand _NeuesElementCommand;
        public ICommand NeuesElementCommand
        {
            get => _NeuesElementCommand;
            set
            {
                if (_NeuesElementCommand != value)
                {
                    _NeuesElementCommand = value;
                    OnPropertyChanged(nameof(NeuesElementCommand));
                }
            }
        }

        private ICommand _LoeschenCommand;
        public ICommand LoeschenCommand
        {
            get => _LoeschenCommand;
            set
            {
                if (_LoeschenCommand != value)
                {
                    _LoeschenCommand = value;
                    OnPropertyChanged(nameof(LoeschenCommand));
                }
            }
        }
        private ICommand _SchliessenCommand;
        public ICommand SchliessenCommand
        {
            get => _SchliessenCommand;
            set
            {
                if (_SchliessenCommand != value)
                {
                    _SchliessenCommand = value;
                    OnPropertyChanged(nameof(SchliessenCommand));
                }
            }
        }   

       
        protected void OnSchließenRequested()
        {
            SchliessenEvent?.Invoke(this, EventArgs.Empty);
        }



        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
