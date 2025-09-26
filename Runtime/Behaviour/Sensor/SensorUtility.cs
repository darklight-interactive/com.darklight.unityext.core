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

            public static Sensor GetOrAddSensor(
                GameObject obj,
                SensorConfig config,
                List<SensorDetectionFilter> filters = null
            )
            {
                Sensor out_sensor = null;

                // << CHECK IF SENSOR ALREADY EXISTS >>
                GetSensorByConfig(obj, config, out Sensor existingSensor);
                if (existingSensor != null)
                {
                    out_sensor = existingSensor;
                }
                else
                {
                    // << CHECK IF AN UNUSED SENSOR EXISTS >>
                    GetAllSensorsOnObject(obj, out Sensor[] sensors);
                    foreach (Sensor s in sensors)
                    {
                        if (s._config == null)
                        {
                            s._config = config;
                            out_sensor = s;
                            break;
                        }
                    }
                }

                // << ADD SENSOR IF NOT FOUND >>
                if (out_sensor == null)
                {
                    out_sensor = obj.AddComponent<Sensor>();
                    out_sensor._config = config;
                }

                // << ADD FILTERS >>
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        out_sensor.GetOrAddDetector(filter, out var detector);
                    }
                }
                return out_sensor;
            }
        }
    }
}
