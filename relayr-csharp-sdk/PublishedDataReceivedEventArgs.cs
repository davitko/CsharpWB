/*
 * Copyright (c) 2014 iThings4U Gmbh
 * 
 * Author:
 *      Peter Dwersteg      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace relayr_csharp_sdk
{
    public class PublishedDataReceivedEventArgs
    {
        public dynamic Data;
        public bool DupFlag;
        public bool Retain;
        public QualityOfService QosLevel;

        public PublishedDataReceivedEventArgs(dynamic data, bool dupFlag, bool retain, QualityOfService qosLevel)
        {
            Data = data;
            DupFlag = dupFlag;
            Retain = retain;
            QosLevel = qosLevel;
        }
    }
}
