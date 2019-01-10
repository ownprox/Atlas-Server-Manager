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
            public WorkerData(Task WorkTask, CancellationTokenSource cancellationToken)
            {
                this.WorkTask = WorkTask;
                this.cancellationToken = cancellationToken;
            }
        };

        private static readonly WorkerData[] Workers = new WorkerData[(int)WorkerType.Max];

        private static void ToggleWorker(WorkerType Index, bool Enabled)
        {
            switch (Index)
            {
                case WorkerType.StatusUpdate:
                    ServerStatus.Working = Enabled;
                    break;
                case WorkerType.ServerMonitor:
                    ServerMonitor.Working = Enabled;
                    break;
                case WorkerType.ServerUpdate:
                    ServerUpdater.Working = Enabled;
                    break;
            }
        }

        public static void Init(AtlasServerManager AtlasMgr, bool StartUpdateCheck)
        {
            Workers[(int)WorkerType.StatusUpdate] = new WorkerData (Task.Run(() => ServerStatus.UpdateStatus(AtlasMgr)), new CancellationTokenSource());
            Workers[(int)WorkerType.ServerMonitor] = new WorkerData(Task.Run(() => ServerMonitor.CheckServerStatus(AtlasMgr)), new CancellationTokenSource());
            if (StartUpdateCheck)
                Workers[(int)WorkerType.ServerUpdate] = new WorkerData(Task.Run(() => ServerUpdater.CheckForUpdates(AtlasMgr)), new CancellationTokenSource());
        }

        public static void StopUpdating()
        {
            if (Workers[(int)WorkerType.ServerUpdate].WorkTask != null)
            {
                ToggleWorker(WorkerType.ServerUpdate, false);
                if (ServerUpdater.UpdateProcess != null && !ServerUpdater.UpdateProcess.HasExited && ServerUpdater.UpdateProcess.Id != 0)
                    ServerUpdater.UpdateProcess.Kill();
                Workers[(int)WorkerType.ServerUpdate].cancellationToken.Cancel();
            }
        }

        public static void DestroyAll()
        {
            ServerStatus.Working = false;
            ServerMonitor.Working = false;
            ServerUpdater.Working = false;
            if (ServerUpdater.UpdateProcess != null && !ServerUpdater.UpdateProcess.HasExited && ServerUpdater.UpdateProcess.Id != 0)
                ServerUpdater.UpdateProcess.Kill();
            Thread.Sleep(1000);
            foreach (WorkerData w in Workers)
            {
                if (w.WorkTask != null) w.cancellationToken.Cancel();
            }
        }

        public static void ForceUpdaterRestart(AtlasServerManager AtlasMgr)
        {
            AtlasMgr.Updating = false;
            StopUpdating();
            ToggleWorker(WorkerType.ServerUpdate, true);
            Workers[(int)WorkerType.ServerUpdate] = new WorkerData(Task.Run(() => ServerUpdater.CheckForUpdates(AtlasMgr)), new CancellationTokenSource());
        }
    }
}