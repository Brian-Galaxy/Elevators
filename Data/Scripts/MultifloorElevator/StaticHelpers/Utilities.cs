using System.Diagnostics;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    internal class Utilities
    {
        public Stopwatch Stopwatch = new Stopwatch();
        internal int frame = -1;
        internal int second = -1;
        public bool Debug = true;

        public void StopWatchReport(string message, float DebugTreshold = 0.5f)
        {
            Stopwatch.Stop();
            double ms = 10000.0 * Stopwatch.ElapsedTicks / Stopwatch.Frequency;
            if (ms >= DebugTreshold) Logging.Instance.WriteLine($"{message} - ms:{ms:F5}");
            Stopwatch.Reset();
        }

        public void Timing()
        {
            if (frame++ == 59) frame = 0;
            if (frame == 0 && second++ == 9) second = 0;
        }

        public static MyObjectBuilder_CubeGrid GridBuilder(IMyCubeBlock parentBlock, MyObjectBuilder_CubeBlock blockObjBuilder, string subtypeName, Vector3D position, Vector3D forward, Vector3D up)
        {
            MyObjectBuilder_CubeGrid gb = new MyObjectBuilder_CubeGrid
            {
                CreatePhysics = true,
                GridSizeEnum = parentBlock.CubeGrid.GridSizeEnum,
                PositionAndOrientation = new MyPositionAndOrientation(position, forward, up)
            };
            gb.CubeBlocks.Add(ObjectBuilder(parentBlock, blockObjBuilder, subtypeName));
            return gb;
        }

        private static MyObjectBuilder_CubeBlock ObjectBuilder(IMyCubeBlock parentBlock, MyObjectBuilder_CubeBlock blockObjBuilder, string subtypeName)
        {
            blockObjBuilder.SubtypeName = subtypeName;
            blockObjBuilder.Owner = parentBlock.OwnerId;
            blockObjBuilder.BuiltBy = parentBlock.OwnerId;
            return blockObjBuilder;
        }
    }
}