using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public class Passengers
    {
        private List<Passenger> PassengersList = new List<Passenger>();
        public int Count => PassengersList.Count;

        public void GetPassengers(MatrixD cabinMatrix, Vector3D cabinPosition)
        {
            Vector3D cabinMin = new Vector3D(-1.6f, 0.90f, -1.6f);
            Vector3D cabinMax = new Vector3D(1.6f, 4.1f, 1.6f);
            BoundingBoxD cabinBB = new BoundingBoxD(cabinMin, cabinMax).TransformFast(cabinMatrix);
            foreach (IMyEntity entity in MyAPIGateway.Entities.GetTopMostEntitiesInBox(ref cabinBB))
            {
                if (entity is IMyCharacter || entity is IMyFloatingObject || entity is IMyInventoryBag)
                {
                    BoundingBoxD entiyBB = new BoundingBoxD(entity.LocalAABB.Min, entity.LocalAABB.Max);
                    if (cabinBB.Contains(entiyBB.TransformFast(entity.WorldMatrix)) == ContainmentType.Contains)
                    {
                        PassengersList.Add(new Passenger(entity, entity.WorldMatrix.Translation - cabinPosition));
                    }
                }
            }
        }

        public void UpdateOffset(long elevatorId)
        {
            PassengersList.ForEach(p => p.SetOffset(elevatorId));
        }

        public void MoveEntity(Vector3D cabinPosition)
        {
            PassengersList.ForEach(p => p.SetPosition(cabinPosition));
        }

        public Passenger FindByEntityId(long entityId)
        {
            return PassengersList.Find(p => p.Entity.EntityId == entityId);
        }

        public void Clear() => PassengersList.Clear();
    }
}