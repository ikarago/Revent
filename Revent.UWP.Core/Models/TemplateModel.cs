using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Appointments;

namespace Revent.UWP.Core.Models
{
    public class TemplateModel
    {
        [PrimaryKey]
        [AutoIncrement]
        public int TemplateId { get; set; }
        public string TemplateColor { get; set; }
        public string TemplateGlyph { get; set; }
        public string TemplateLabel1 { get; set; }
        public string TemplateLabel2 { get; set; }
        public string TemplateLabel3 { get; set; }
        public string TemplateName { get; set; }

        public bool AppointmentAllDay { get; set; }
        public AppointmentBusyStatus AppointmentBusy { get; set; }
        public string AppointmentDetails { get; set; }
        /// <summary>
        /// Duration is saved in minutes, just sayin' :P
        /// </summary>
        public double AppointmentDuration { get; set; }
        public string AppointmentLocation { get; set; }
        public string AppointmentMeetingLink { get; set; }
        public TimeSpan AppointmentReminder { get; set; }
        public AppointmentSensitivity AppointmentSensitivity { get; set; }
        public DateTimeOffset AppointmentStartTime { get; set; }
        public string AppointmentSubject { get; set; }
    }
}
