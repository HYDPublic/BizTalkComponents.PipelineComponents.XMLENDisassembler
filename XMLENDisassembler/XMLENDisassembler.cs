using System;
using System.Collections.Generic;
using System.Collections;
using System.Resources;
using System.Drawing;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.ScalableTransformation;
using Microsoft.XLANGs.RuntimeTypes;
using System.ComponentModel.DataAnnotations;
using BizTalkComponents.Utils;

namespace BizTalkComponents.PipelineComponents
{
    /// <summary>
    /// Envelope normalizer finds and processes all specified documents and ignores the rest, unwanted elements are also ignored
    /// </summary>
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    [System.Runtime.InteropServices.Guid("35A34C0D-8D73-45fd-960D-DB365CD56388")]
    public partial class XMLENDisassembler : IDisassemblerComponent, IProbeMessage, IComponentUI
    {
        private const string _systemPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2003/system-properties";
        private XmlReader childReader = null;
        private IBaseMessage baseMsg = null;
        public Dictionary<MessageType, IDocumentSpec> messageTypes = new Dictionary<MessageType, IDocumentSpec>();
        private IDocumentSpec currDocument = null;
        XmlDasmComp innerDasm = new XmlDasmComp();
        private SchemaList envelopeSpecNames = new SchemaList();
        private SchemaList documentSpecNames = new SchemaList();
       
        #region Xml disassembler Related Properties
       
        [Description("Document(s) of interest, other types will be ignored")]
        [DisplayName("Document fully qualified name(s)")]
        [RequiredRuntime]
        public SchemaList DocumentSpecNames {
            get
            {
                return documentSpecNames;
            }
            set
            {
                documentSpecNames = value;
            }
        }

        [DisplayName("Envelope fully qualified name")]
        public SchemaList EnvelopeSpecNames
        {
            get
            {
                return envelopeSpecNames;
            }
            set
            {
                envelopeSpecNames = value;
            }
        }

        
        public bool RecoverableInterchangeProcessing { get; set; }
        public bool ValidateDocument { get; set; }


        #endregion

        void IDisassemblerComponent.Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
        {
           
            innerDasm.EnvelopeSpecNames = this.EnvelopeSpecNames;
            //Important not to add the DocumentSpecNames to the inner disassembler as it will not allow messages
            //that we want to ignore, that is not the same as AllowUnrecognizedMessage = true as UnrecognizedMessage
            //means a message without corresponding installed schema
            //This is also why i could not sublass XmlDasmComp
            innerDasm.AllowUnrecognizedMessage = true;
            innerDasm.ValidateDocument = this.ValidateDocument;
            innerDasm.RecoverableInterchangeProcessing = this.RecoverableInterchangeProcessing;

            SchemaList documents = this.DocumentSpecNames;

            foreach (Schema item in documents)
            {
                IDocumentSpec documentSpec = pContext.GetDocumentSpecByName(item.SchemaName);

                string[] messageParts = documentSpec.DocType.Split(new char[]{'#'});

                MessageType msgType = new MessageType
                {
                    RootName = messageParts[1]
                    ,
                    TargetNamespace = messageParts[0]
                };

                if (messageTypes.ContainsKey(msgType) == false)
                {
                   
                    messageTypes.Add(msgType, documentSpec);
                }
            }

        
            innerDasm.Disassemble(pContext, pInMsg);

           
        }

        IBaseMessage CreateMessage(IPipelineContext pContext,XmlReader reader)
        {
            IBaseMessage outMsg = pContext.GetMessageFactory().CreateMessage();
            outMsg.AddPart("Body", pContext.GetMessageFactory().CreateMessagePart(), true);
            outMsg.Context = baseMsg.Context;

            outMsg.Context.Promote("MessageType", _systemPropertiesNamespace, currDocument.DocType);
            //we add the SchemaStrongName as there could posssible exist multiple messages with the same MessageType
            outMsg.Context.Promote("SchemaStrongName", _systemPropertiesNamespace
            , currDocument.DocSpecStrongName);
            //Is used XmlTranslatorStream as it is the only one i could find that takes an XmlReader as input
            //At this point i did not want to write my own custom stream
            outMsg.BodyPart.Data = new XmlTranslatorStream(reader.ReadSubtree());

            return outMsg;
        }
        
        IBaseMessage GetNextMessage(IPipelineContext pContext)
        {
            IBaseMessage outMsg = null;

            if (childReader == null || childReader.EOF)
            {

                baseMsg = innerDasm.GetNext(pContext);

                if (baseMsg == null)
                    return baseMsg;

                childReader = XmlReader.Create(baseMsg.BodyPart.GetOriginalDataStream());

            }

            while (childReader.Read())
            {
                MessageType msgType = new MessageType
                {
                    RootName = childReader.LocalName
                    ,
                    TargetNamespace = childReader.NamespaceURI
                };

                if(messageTypes.ContainsKey(msgType))
                {
                    
                    currDocument = messageTypes[msgType];
                    //Is used XmlTranslatorStream as it is the only one i could find that takes an XmlReader as input
                    //At this point i did not want to write my own custom stream
                   // XmlTranslatorStream trans = new XmlTranslatorStream(childReader.ReadSubtree());

                    outMsg = CreateMessage(pContext, childReader);
                    break;
                }
            }

            if (outMsg == null)
                outMsg = GetNextMessage(pContext);//make sure there is no more messages in base queue

            return outMsg;//return null if the message is null, but check if there exists more enveloped messages
        }
        IBaseMessage IDisassemblerComponent.GetNext(IPipelineContext pContext)
        {
            return GetNextMessage(pContext);
        }


        public  IEnumerator Validate(object projectSystem)
        {
            return null;
        }

        public  bool Probe(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            return true;
        }

        [Browsable(false)]
        public IntPtr Icon
        {
            get
            {
                return innerDasm.Icon;
            }
        }
    } 
}
