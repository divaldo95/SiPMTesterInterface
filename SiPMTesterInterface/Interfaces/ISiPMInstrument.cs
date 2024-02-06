using System;
using NetMQ.Sockets;
using SiPMTesterInterface.Enums;

namespace SiPMTesterInterface.Interfaces
{
	public interface ISiPMInstrument
	{
        public void RunCommand(string command);
	}
}

