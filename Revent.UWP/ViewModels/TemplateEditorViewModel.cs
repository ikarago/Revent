using Revent.UWP.Core.Models;
using Revent.UWP.Helpers;
using Revent.UWP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revent.UWP.ViewModels
{
    public class TemplateEditorViewModel : Observable
    {
        // Properties
        private TemplateModel _template;
        public TemplateModel Template
        {
            get { return _template; }
            set { Set(ref _template, value); }
        }

        // Keep the imported or old model temporarily for comparison
        private TemplateModel _existingTemplate;
        public TemplateModel ExistingTemplate
        {
            get { return _existingTemplate; }
            set { Set(ref _existingTemplate, value); }
        }

        // UI Properties
        private bool _uiIsTemplateNameEmpty;
        public bool UiIsTemplateNameEmpty
        {
            get { return _uiIsTemplateNameEmpty; }
            set { Set(ref _uiIsTemplateNameEmpty, value); }
        }

        private bool _uiIsSubjectEmpty;
        public bool UiIsSubjectEmpty
        {
            get { return _uiIsSubjectEmpty; }
            set { Set(ref _uiIsSubjectEmpty, value); }
        }


        // Constructor
        public TemplateEditorViewModel(TemplateModel existModel = null)
        {
            Initialize(existModel);
        }
        // Initialize
        private void Initialize(TemplateModel existModel = null)
        {
            // Check for a previous model (when editing or importing)
            // NOTE: Imported templated get an ID of 0 to indicate this method it hasn't been saved to the database yet
            if (existModel == null)
            {
                Template = new TemplateModel();
            }
            else
            {
                Template = existModel;
                ExistingTemplate = existModel;
            }

            // Set UI Empty toggles so the error messages won't be displayed right away
            UiIsSubjectEmpty = false;
            UiIsTemplateNameEmpty = false;

        }



        // Commands



        // Methods
        public TemplateModel SaveTemplate()
        {
            TemplateModel savedModel = new TemplateModel();
            // Save to the database
            try
            {
                savedModel = DatabaseService.Write(Template);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TemplateEditorViewModel - SaveTemplate:" + ex);
            }

            // Return the TemplateModel to the Dialog that has called it
            // This saved TemplateModel also contains the ID, so when it get's added back by the MainViewModel, it'll work properly if updated in the same usersession
            return savedModel;
        }

        /// <summary>
        /// Checks if the required things have been filled in
        /// </summary>
        /// <returns>boolean indicating if all required input has been entered (true) or not (false)</returns>
        public bool CheckForRequiredInput()
        {
            bool isAllGood = true;
            // Check if the Template Name has been filled in
            if (Template.TemplateName == "" || Template.TemplateName == null)
            {
                UiIsTemplateNameEmpty = true;
                isAllGood = false;
            }
            // Check if the Subject has been filled in
            if (Template.AppointmentSubject == "" || Template.AppointmentSubject == null)
            {
                UiIsSubjectEmpty = true;
                isAllGood = false;
            }

            // If one of the checks has failed, this returns false, otherwise it returns true
            return isAllGood;
        }


    }
}
