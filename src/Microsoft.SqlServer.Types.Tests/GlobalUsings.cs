﻿global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.IO;
global using System.Runtime.Serialization;
global using System.Text;

global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using Microsoft.SqlServer.Types.SqlHierarchy;

global using System.Data.SqlTypes;
#if NETFRAMEWORK
global using Microsoft.SqlServer.Server;
global using System.Data.SqlClient;
#else
global using Microsoft.Data.SqlClient.Server;
global using Microsoft.Data.SqlClient;
#endif