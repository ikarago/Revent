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

                /*if (Template.TemplateId >= 1)
                {
                    // Update existing model

                }
                else
                {
                    // Create new Model
                    saved
                }*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TemplateEditorViewModel - SaveTemplate:" + ex);
            }

            // Return the TemplateModel to the Dialog that has called it
            // This saved TemplateModel also contains the ID, so when it get's added back by the MainViewModel, it'll work properly if updated in the same usersession
            return savedModel;
        }


    }
}
