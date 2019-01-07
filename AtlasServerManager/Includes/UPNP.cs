using NATUPNPLib;

namespace AtlasServerManager.Includes
{
	public class UPNP
	{
        private static string _LocalIPAddress = string.Empty;
        private static string LocalIPAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_LocalIPAddress))
                {
                    System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    if (host.AddressList.Length > 0)_LocalIPAddress = host.AddressList[0].ToString();
                }
                return _LocalIPAddress;
            }
        }

        private static UPnPNAT UpnpNat = null;
        private static IStaticPortMappingCollection UpnpMap = null;

		private static void Init()
		{
            if (UpnpNat == null) UpnpNat = new NATUPNPLib.UPnPNAT();
            if (UpnpMap == null) UpnpMap = UpnpNat.StaticPortMappingCollection;
		}

		public static bool AddUPNPServer(int ServerPort, int QueryPort, string AltSaveDir)
        {
            Init();
            if (UpnpMap == null)
            {
                AtlasServerManager.GetInstance().Log("[Auto Port Forwarding] UPNP Does not seeem enabled at the router admins interface");
                return false;
            }
            foreach (IStaticPortMapping EMaps in UpnpMap) if (EMaps.ExternalPort == ServerPort || EMaps.ExternalPort == QueryPort) return false;
			UpnpMap.Add(ServerPort, "UDP", ServerPort, LocalIPAddress, true, "Atlas Server: " + AltSaveDir);
			UpnpMap.Add(QueryPort, "UDP", QueryPort, LocalIPAddress, true, "Atlas Query: " + AltSaveDir);
            return true;
		}

		public static void RemoveUPNPServer(int ServerPort, int QueryPort)
        {
            if (UpnpMap == null) return;
            UpnpMap.Remove(ServerPort, "UDP");
            UpnpMap.Remove(QueryPort, "UDP");
		}

        public static void Destroy()
        {
            if (UpnpNat != null && UpnpMap != null)
            {
                foreach (IStaticPortMapping EMaps in UpnpMap)
                    if (EMaps.Description.StartsWith("Atlas ")) UpnpMap.Remove(EMaps.ExternalPort, "UDP");
            }
        }
    }
}