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

            public static void GetSensorByConfig(GameObject obj, Config config, out Sensor sensor)
            {
                sensor = obj.GetComponentsInChildren<Sensor>(true)
                    .FirstOrDefault(s => s._config == config);
            }

            public static Sensor GetOrAddSensor(
                GameObject obj,
                Config config,
                List<DetectionFilter> filters = null
            )
            {
                // << CHECK IF SENSOR ALREADY EXISTS >>
                GetSensorByConfig(obj, config, out Sensor existingSensor);
                if (existingSensor != null)
                {
                    return existingSensor;
                }

                // << ADD SENSOR >>
                Sensor sensor = obj.AddComponent<Sensor>();
                sensor._config = config;

                // << ADD FILTERS >>
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        sensor.GetOrAddDetector(filter, out var detector);
                    }
                }
                return sensor;
            }
        }
    }
}
