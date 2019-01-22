using System.Threading;
using System.Threading.Tasks;

namespace AtlasServerManager.Includes
{
    class Worker
    {
        public enum WorkerType
        {
            StatusUpdate = 0,
            ServerMonitor,
            ServerUpdate,
            Max,
        }

        struct WorkerData
        {
            public Task WorkTask;
            public CancellationTokenSource cancellationToken;
            public WorkerData(System.Action WorkAction, CancellationTokenSource cancellationToken)
            {
                this.cancellationToken = cancellationToken;
                WorkTask = Task.Run(WorkAction, cancellationToken.Token);
            }
        };

        private static readonly WorkerData[] Workers = new WorkerData[(int)WorkerType.Max];

        public static void AddWorker(WorkerType Index, AtlasServerManager AtlasMgr)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            var Token = cancellationToken.Token;
            switch (Index)
            {
                case WorkerType.StatusUpdate:
                    Workers[(int)WorkerType.StatusUpdate] = new WorkerData(() => ServerStatus.UpdateStatus(AtlasMgr, Token), cancellationToken);
                    break;
                case WorkerType.ServerMonitor:
                    Workers[(int)WorkerType.ServerMonitor] = new WorkerData(() => ServerMonitor.CheckServerStatus(AtlasMgr, Token), cancellationToken);
                    break;
                case WorkerType.ServerUpdate:
                    Workers[(int)WorkerType.ServerUpdate] = new WorkerData(() => ServerUpdater.CheckForUpdates(AtlasMgr, Token), cancellationToken);
                    break;
            }
        }

        public static void Init(AtlasServerManager AtlasMgr, bool StartUpdateCheck)
        {
            AddWorker(WorkerType.StatusUpdate, AtlasMgr);
            if (StartUpdateCheck) {
                ServerUpdater.Updating = true;
                ServerUpdater.ForcedUpdate = true;
                AddWorker(WorkerType.ServerUpdate, AtlasMgr);
            }

            AddWorker(WorkerType.ServerMonitor, AtlasMgr);
            

        }

        public static void StopUpdating()
        {
            ServerUpdater.Updating = false;
            if (Workers[(int)WorkerType.ServerUpdate].WorkTask != null)
            {
                Workers[(int)WorkerType.ServerUpdate].cancellationToken.Cancel();
                if (ServerUpdater.UpdateProcess != null && !ServerUpdater.UpdateProcess.HasExited && ServerUpdater.UpdateProcess.Id != 0)
                    ServerUpdater.UpdateProcess.Kill();
            }
        }

        public static void DestroyAll()
        {
            foreach (WorkerData w in Workers)
            {
                if (w.WorkTask != null) w.cancellationToken.Cancel();
            }
            if (ServerUpdater.UpdateProcess != null && !ServerUpdater.UpdateProcess.HasExited && ServerUpdater.UpdateProcess.Id != 0)
                ServerUpdater.UpdateProcess.Kill();
            Thread.Sleep(1000);
        }

        public static void ForceUpdaterRestart(AtlasServerManager AtlasMgr)
        {
            StopUpdating();
            ServerUpdater.Updating = true;
            AddWorker(WorkerType.ServerUpdate, AtlasMgr);
        }
    }
}