using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Darklight.Behaviour.Sensor
{
    public static class SensorUtility
    {
        public static void GetAllSensorsOnObject(GameObject obj, out SensorBase[] sensors)
        {
            sensors = obj.GetComponentsInChildren<SensorBase>();
        }

        public static void GetAllSensorsInChildren(GameObject obj, out SensorBase[] sensors)
        {
            sensors = obj.GetComponentsInChildren<SensorBase>(true);
        }

        public static void GetSensorByConfig(
            GameObject obj,
            SensorConfig config,
            out SensorBase sensor
        )
        {
            sensor = obj.GetComponentsInChildren<SensorBase>(true)
                .FirstOrDefault(s => s.Config == config);
        }

        public static SensorBase GetOrAddSensor(
            GameObject obj,
            SensorConfig config,
            List<SensorDetectionFilter> filters = null
        )
        {
            // << CHECK IF SENSOR ALREADY EXISTS >>
            GetSensorByConfig(obj, config, out SensorBase existingSensor);
            if (existingSensor != null)
            {
                return existingSensor;
            }

            // << ADD SENSOR >>
            SensorBase sensor = obj.AddComponent<SensorBase>();
            sensor.Config = config;

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
