using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.Services;
using Revent.UWP.Views.Dialogs;

namespace Revent.UWP.ViewModels
{
    public class MainViewModel : Observable
    {

        // Properties
        private ObservableCollection<TemplateModel> _templates;
        public ObservableCollection<TemplateModel> Templates
        {
            get { return _templates; }
            set { Set(ref _templates, value); }
        }

        private ObservableCollection<TemplateModel> _templatesSearchResults;
        public ObservableCollection<TemplateModel> TemplatesSearchResults
        {
            get { return _templatesSearchResults; }
            set { Set(ref _templatesSearchResults, value); }
        }

        private TemplateModel _selectedTemplate;
        public TemplateModel SelectedTemplate
        {
            get { return _selectedTemplate; }
            set { Set(ref _selectedTemplate, value); }
        }



        // Constructor
        public MainViewModel()
        {
            Initialize();
        }
        // Initialize
        private void Initialize()
        {
            // Set an empty list to populate with the Templates
            // If we don't do this and the method of getting the templates fails, the app is guaranteed to do boom
            _templates = new ObservableCollection<TemplateModel>();

            // Get the Templates from the database
            GetTemplates();

            // Make sure no template is selected
            _selectedTemplate = null;
        }



        // Commands
        private ICommand _newTemplateCommand;
        public ICommand NewTemplateCommand
        {
            get
            {
                if (_newTemplateCommand == null)
                {
                    _newTemplateCommand = new RelayCommand(
                        () =>
                        {
                            NewTemplate();
                        });
                }
                return _newTemplateCommand;
            }
        }

        private ICommand _importTemplateCommand;
        public ICommand ImportTemplateCommand
        {
            get
            {
                if (_importTemplateCommand == null)
                {
                    _importTemplateCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _importTemplateCommand;
            }
        }

        private ICommand _editTemplateCommand;
        public ICommand EditTemplateCommand
        {
            get
            {
                if (_editTemplateCommand == null)
                {
                    _editTemplateCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _editTemplateCommand;
            }
        }

        private ICommand _deleteTemplateCommand;
        public ICommand DeleteTemplateCommand
        {
            get
            {
                if (_deleteTemplateCommand == null)
                {
                    _deleteTemplateCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _deleteTemplateCommand;
            }
        }

        private ICommand _openTemplateCommand;
        public ICommand OpenTemplateCommand
        {
            get
            {
                if (_openTemplateCommand == null)
                {
                    _openTemplateCommand = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _openTemplateCommand;
            }
        }

        private ICommand _pinTemplateToStart;
        public ICommand PinTemplateToStart
        {
            get
            {
                if (_pinTemplateToStart == null)
                {
                    _pinTemplateToStart = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _pinTemplateToStart;
            }
        }

        private ICommand _unpinTemplateToStart;
        public ICommand UnpinTemplateToStart
        {
            get
            {
                if (_unpinTemplateToStart == null)
                {
                    _unpinTemplateToStart = new RelayCommand(
                        () =>
                        {
                            // #TODO
                        });
                }
                return _unpinTemplateToStart;
            }
        }



        // Methods
        private void GetTemplates()
        {
            try
            {
                Templates = DatabaseService.GetTemplates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainViewModel - GetTemplates - Could not get templates. Exception in the line below:");
                Debug.WriteLine(ex.Message);
            }
        }


        private async void NewTemplate()
        {
            // Open the NewTemplate Dialog and get the TemplateModel made in there
            NewTemplateDialog temp = new NewTemplateDialog();
            await temp.ShowAsync();
            TemplateModel model = temp.SavedTemplate;

            // Add this model back to the Templates List for use
            Templates.Add(model);
        }


        // Open Template





    }
}
