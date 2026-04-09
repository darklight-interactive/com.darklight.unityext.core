using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.Behaviour
{
    public partial class Sensor
    {
        public static class Utility
        {
            public static void GetAllSensorsOnObject(GameObject obj, out Sensor[] sensors)
            {
                sensors = obj.GetComponentsInChildren<Sensor>();
            }

            public static void GetAllSensorsInChildren(GameObject obj, out Sensor[] sensors)
            {
                sensors = obj.GetComponentsInChildren<Sensor>(true);
            }

            public static void GetSensorByConfig(
                GameObject obj,
                SensorConfig config,
                out Sensor sensor
            )
            {
                sensor = obj.GetComponentsInChildren<Sensor>(true)
                    .FirstOrDefault(s => s._config == config);
            }
        }
    }
}
