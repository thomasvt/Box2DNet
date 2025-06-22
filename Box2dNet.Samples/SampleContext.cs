namespace Box2dNet.Samples
{
    internal class SampleContext
    {
        public void Save()
        {}
        public void Load()
        {}

        // struct GLFWwindow* window = nullptr;
        public float uiScale = 1.0f;
        public float hertz = 60.0f;
        public int subStepCount = 4;
        public int workerCount = 1;
        public bool restart = false;
        public bool pause = false;
        public bool singleStep = false;
        public bool useCameraBounds = false;
        public bool drawJointExtras = false;
        public bool drawBounds = false;
        public bool drawMass = false;
        public bool drawBodyNames = false;
        public bool drawContactPoints = false;
        public bool drawContactNormals = false;
        public bool drawContactImpulses = false;
        public bool drawContactFeatures = false;
        public bool drawFrictionImpulses = false;
        public bool drawIslands = false;
        public bool drawGraphColors = false;
        public bool drawCounters = false;
        public bool drawProfile = false;
        public bool enableWarmStarting = true;
        public bool enableContinuous = true;
        public bool enableSleep = true;

        // These are persisted
        public int sampleIndex = 0;
        public bool drawShapes = true;
        public bool drawJoints = true;
    }
}
