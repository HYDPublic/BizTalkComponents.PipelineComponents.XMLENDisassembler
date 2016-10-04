using System;
using System.Collections.Generic;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.ScalableTransformation;
using Microsoft.XLANGs.RuntimeTypes;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Utilities;
using propertyHelper = Microsoft.BizTalk.Component.PropertyHelper;

namespace BizTalkComponents.PipelineComponents
{
    /// <summary>
    ///  Extended functionality
    ///  1) Normalize envelope path, by removing invalid child records
    /// </summary>
    public partial class XMLENDisassembler: IBaseComponent,IPersistPropertyBag
    {
      
      
        #region IBaseComponent Members

        public  string Description
        {
            get { return "Envelope Normalize XML Disassembler"; }
        }

        public  string Name
        {
            get { return "Envelope Normalize Disassembler"; }
        }

        public  string Version
        {
            get { return "1.0.0"; }
        }

        #endregion

        void IPersistPropertyBag.GetClassID(out Guid classID)
        {
            classID = new Guid("35A34C0D-8D73-45fd-960D-DB365CD56388");
        }

        void IPersistPropertyBag.InitNew()
        {
            return;
        }
        /// <summary>
        /// Loads configuration property for component.
        /// </summary>
        /// <param name="pb">Configuration property bag.</param>
        /// <param name="errlog">Error status (not used in this code).</param>
        void IPersistPropertyBag.Load(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, Int32 errlog)
        {

            string strEnvelopes = (string)propertyHelper.ReadPropertyBag(pb, "EnvelopeSpecNames");
            if (strEnvelopes != null && strEnvelopes.Length > 0)
            {
               
                string[] strEnvArray = strEnvelopes.Split('|');
               
                this.envelopeSpecNames.Clear();
                for (int lowerBound = strEnvArray.GetLowerBound(0); lowerBound <= strEnvArray.GetUpperBound(0); ++lowerBound)
                {
                    Schema schema = new Schema(strEnvArray[lowerBound]);
                    this.envelopeSpecNames.Add(schema);
                }
               
            }

            string strDocuments = (string)propertyHelper.ReadPropertyBag(pb, "DocumentSpecNames");
            if (strDocuments != null && strDocuments.Length > 0)
            {

                string[] strDocArray = strDocuments.Split('|');

                this.DocumentSpecNames.Clear();
                for (int lowerBound = strDocArray.GetLowerBound(0); lowerBound <= strDocArray.GetUpperBound(0); ++lowerBound)
                {
                    Schema schema = new Schema(strDocArray[lowerBound]);
                    this.DocumentSpecNames.Add(schema);
                }

            }
            
            this.RecoverableInterchangeProcessing = PropertyBagHelper.ReadPropertyBag<Boolean>(pb, "RecoverableInterchangeProcessing", this.RecoverableInterchangeProcessing);
            this.ValidateDocument = PropertyBagHelper.ReadPropertyBag<Boolean>(pb, "ValidateDocument", this.ValidateDocument);
            
           
        }

        /// <summary>
        /// Saves current component configuration into the property bag.
        /// </summary>
        /// <param name="pb">Configuration property bag.</param>
        /// <param name="fClearDirty">Not used.</param>
        /// <param name="fSaveAllProperties">Not used.</param>
        void IPersistPropertyBag.Save(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
        {
            string envelopeSpecNames = String.Empty;
            foreach (var item in this.EnvelopeSpecNames)
            {
                if(envelopeSpecNames == String.Empty)
                {
                    envelopeSpecNames = item.SchemaName;
                    continue;
                }

                envelopeSpecNames += String.Format("|{0}",item.SchemaName);
               
            }

            string documentSpecNames = String.Empty;
            foreach (var item in this.documentSpecNames)
            {
                if (documentSpecNames == String.Empty)
                {
                    documentSpecNames = item.SchemaName;
                    continue;
                }

                documentSpecNames += String.Format("|{0}", item.SchemaName);

            }
            PropertyBagHelper.WritePropertyBag(pb, "DocumentSpecNames", documentSpecNames);
            PropertyBagHelper.WritePropertyBag(pb, "EnvelopeSpecNames", envelopeSpecNames);
            PropertyBagHelper.WritePropertyBag(pb, "RecoverableInterchangeProcessing", this.RecoverableInterchangeProcessing);
            PropertyBagHelper.WritePropertyBag(pb, "ValidateDocument", this.ValidateDocument);
          
            
         

        }

        
    }

}
