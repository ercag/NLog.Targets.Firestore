using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Api.Gax;
using Google.Cloud.Firestore;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;


namespace NLog.Targets.Firestore
{
    [Target("Firestore")]
    public class FirestoreTarget : TargetWithLayout
    {
        #region Layout Members
        [RequiredParameter]
        public Layout FirestoreType { get; set; }

        [RequiredParameter]
        public Layout ProjectId { get; set; }

        [RequiredParameter]
        public Layout DatabaseId { get; set; }

        [RequiredParameter]
        public Layout PrivateKeyId { get; set; }

        [RequiredParameter]
        public Layout PrivateKey { get; set; }

        [RequiredParameter]
        public Layout ClientEmail { get; set; }

        [RequiredParameter]
        public Layout ClientId { get; set; }

        //[RequiredParameter]
        public Layout AuthUri { get; set; }

        //[RequiredParameter]
        public Layout TokenUri { get; set; }

        //[RequiredParameter]
        public Layout AuthProviderX509CertUrl { get; set; }

        //[RequiredParameter]
        public Layout ClientX509CertUrl { get; set; }

        [RequiredParameter]
        public Layout Collection { get; set; }

        public Layout ExcludeLoggers { get; set; }
        #endregion

        #region Firestore Members
        FirestoreDb Db;
        #endregion
        #region Local Members
        string[] excludeLoggers
        {
            get
            {
                if (ExcludeLoggers == null)
                {
                    return null;
                }
                else
                {
                    return ExcludeLoggers.Render(LogEventInfo.CreateNullEvent()).Split(",");
                }
            }
        }
        #endregion

        #region Firestore Init Methods
        public FirestoreDb Firestore()
        {
            return new FirestoreDbBuilder
            {
                ProjectId = ProjectId.Render(LogEventInfo.CreateNullEvent()),
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                JsonCredentials = FirestoreSettingsJson(),
                DatabaseId = DatabaseId.Render(LogEventInfo.CreateNullEvent())
            }.Build();
        }
        private string FirestoreSettingsJson()
        {
            return JsonSerializer.Serialize(LayoutToFirestoreSettingsMap());
        }
        #endregion

        #region NLog Settings Map To Firestore Settings
        private FirestoreSettings LayoutToFirestoreSettingsMap()
        {

            return new FirestoreSettings()
            {
                ProjectId = ProjectId.Render(LogEventInfo.CreateNullEvent()),
                ClientId = ClientId.Render(LogEventInfo.CreateNullEvent()),
                PrivateKeyId = PrivateKeyId.Render(LogEventInfo.CreateNullEvent()),
                PrivateKey = PrivateKey.Render(LogEventInfo.CreateNullEvent()),
                ClientEmail = ClientEmail.Render(LogEventInfo.CreateNullEvent()),
                AuthUri = AuthUri == null ? null : AuthUri.Render(LogEventInfo.CreateNullEvent()),
                FirestoreType = FirestoreType.Render(LogEventInfo.CreateNullEvent()),
                TokenUri = TokenUri == null ? null : TokenUri.Render(LogEventInfo.CreateNullEvent()),
                AuthProviderX509CertUrl = AuthProviderX509CertUrl == null ? null : AuthProviderX509CertUrl.Render(LogEventInfo.CreateNullEvent()),
                ClientX509CertUrl = ClientX509CertUrl == null ? null : ClientX509CertUrl.Render(LogEventInfo.CreateNullEvent()),
                DatabaseId = DatabaseId.Render(LogEventInfo.CreateNullEvent())
            };
        }
        #endregion

        #region Firestore Methods

        /// <summary>
        /// Sync structure
        /// </summary>
        /// <param name="logEvent">For sync parameters</param>
        private void WriteToFirestore(LogEventInfo logEvent)
        {
            var data = new Dictionary<string, object>
            {
                { "id", $"{logEvent.SequenceID}"},
                { "time", $"{logEvent.TimeStamp.ToString("dd.MM.yyyy HH:mm:ss")}"},
                { "level", $"{logEvent.Level}"},
                { "message", $"{logEvent.FormattedMessage}"},
                { "logger", $"{logEvent.LoggerName}"},
                { "callsite", $"{logEvent.CallerMemberName} - {logEvent.CallerClassName} - {logEvent.CallerLineNumber}\n{logEvent.CallerFilePath}"},
                { "exception", $"{logEvent.Exception}"},
                { "parameters", $"{logEvent.Parameters}"},
                { "properties", $"{logEvent.Properties}"},
            };
            DocumentReference docRef = Db.Collection(Collection.Render(LogEventInfo.CreateNullEvent())).Document(Guid.NewGuid().ToString());
            //var collection = docRef.Collection(DateTime.Now.ToString("yyyy-MM-dd")).Document(DateTime.Now.ToString("HH:mm:ss:fff"));//.AddAsync(data).GetAwaiter().GetResult();

            var result = docRef.SetAsync(data, SetOptions.MergeAll).GetAwaiter().GetResult();
        }
        #endregion

        #region Overrides
        protected override void InitializeTarget()
        {
            Db = Firestore();

            base.InitializeTarget();
        }
        protected override void CloseTarget()
        {
            Db = null;
            base.CloseTarget();
        }
        protected override void Write(LogEventInfo logEvent)
        {
            if (excludeLoggers != null)
            {
                foreach (string item in excludeLoggers)
                {
                    if (logEvent.LoggerName.Contains(item))
                        return;
                }
            }

            WriteToFirestore(logEvent);
            base.Write(logEvent);
        }

        #region Dispose Pattern
        protected override void Dispose(bool disposing)
        {
            Db = null;
            base.Dispose(disposing);
        }
        #endregion
        #endregion
    }
}
