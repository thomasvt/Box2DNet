using System.Numerics;
using Box2dNet.Interop;

namespace Box2dNet.Samples
{
    internal class Sample : IDisposable
    {

        public bool BENCHMARK_DEBUG = false;

        public Sample(SampleContext context)
        {
            m_context = context;
            // m_camera = context.camera;
            // m_draw = context.draw;

            //m_scheduler = new enki::TaskScheduler;
            //m_scheduler->Initialize(m_context->workerCount);

            //m_tasks = new SampleTask[m_maxTasks];
            //m_taskCount = 0;

            m_threadCount = 1 + m_context.workerCount;

            m_worldId = default;

            m_textLine = 30;
            m_textIncrement = 22;
            m_mouseJointId = default;

            m_stepCount = 0;

            m_groundBodyId = default;

            m_maxProfile = default;
            m_totalProfile = default;

            // g_randomSeed = RAND_SEED;

            CreateWorld();
            // TestMathCpp();
        }

        public virtual CameraSettings InitialCameraSettings => new(Vector2.Zero, 200);

        public void Dispose()
        {
            // By deleting the world, we delete the bomb, mouse joint, etc.
            B2Api.b2DestroyWorld(m_worldId);
        }

        public void CreateWorld()
        {
            if (m_worldId != default)
            {
                B2Api.b2DestroyWorld(m_worldId);
                m_worldId = default;
            }

            var worldDef = m_context.workerCount > 1 ? B2Api.b2DefaultWorldDef_WithDotNetTpl(m_context.workerCount) : B2Api.b2DefaultWorldDef();
            worldDef.enableSleep = m_context.enableSleep;
            m_worldId = B2Api.b2CreateWorld(worldDef);
        }

        //public void DrawTitle(string title)
        //{
        //    m_context.draw.DrawString(new (5,5), title);
        //    m_textLine = 26;
        //}

        public virtual void Step(Box2dMeshDrawer meshDrawer)
        {
            float timeStep = m_context.hertz > 0.0f ? 1.0f / m_context.hertz : 0.0f;

            if (m_context.pause)
            {
                if (m_context.singleStep)
                {
                    m_context.singleStep = false;
                }
                else
                {
                    timeStep = 0.0f;
                }

                //if (m_context.draw.m_showUI)
                //{
                //    DrawTextLine("****PAUSED****");
                //    m_textLine += m_textIncrement;
                //}
            }

            //m_context.draw.m_debugDraw.drawingBounds = m_context.camera.GetViewBounds();
            //m_context.draw.m_debugDraw.useDrawingBounds = m_context.useCameraBounds;
            //m_context.draw.m_debugDraw.drawShapes = m_context.drawShapes;
            //m_context.draw.m_debugDraw.drawJoints = m_context.drawJoints;
            //m_context.draw.m_debugDraw.drawJointExtras = m_context.drawJointExtras;
            //m_context.draw.m_debugDraw.drawBounds = m_context.drawBounds;
            //m_context.draw.m_debugDraw.drawMass = m_context.drawMass;
            //m_context.draw.m_debugDraw.drawBodyNames = m_context.drawBodyNames;
            //m_context.draw.m_debugDraw.drawContactPoints = m_context.drawContactPoints;
            //m_context.draw.m_debugDraw.drawGraphColors = m_context.drawGraphColors;
            //m_context.draw.m_debugDraw.drawContactNormals = m_context.drawContactNormals;
            //m_context.draw.m_debugDraw.drawContactImpulses = m_context.drawContactImpulses;
            //m_context.draw.m_debugDraw.drawContactFeatures = m_context.drawContactFeatures;
            //m_context.draw.m_debugDraw.drawFrictionImpulses = m_context.drawFrictionImpulses;
            //m_context.draw.m_debugDraw.drawIslands = m_context.drawIslands;

            B2Api.b2World_EnableSleeping(m_worldId, m_context.enableSleep);
            B2Api.b2World_EnableWarmStarting(m_worldId, m_context.enableWarmStarting);
            B2Api.b2World_EnableContinuous(m_worldId, m_context.enableContinuous);

            for (int i = 0; i < 1; ++i)
            {
                B2Api.b2World_Step(m_worldId, timeStep, m_context.subStepCount);
                // m_taskCount = 0;
            }

            meshDrawer.DrawWorld(m_worldId);

            if (timeStep > 0.0f)
            {
                ++m_stepCount;
            }

            if (m_context.drawCounters)
            {
                b2Counters s = B2Api.b2World_GetCounters(m_worldId);

                //DrawTextLine("bodies/shapes/contacts/joints = %d/%d/%d/%d", s.bodyCount, s.shapeCount, s.contactCount, s.jointCount);
                //DrawTextLine("islands/tasks = %d/%d", s.islandCount, s.taskCount);
                //DrawTextLine("tree height static/movable = %d/%d", s.staticTreeHeight, s.treeHeight);

                //int totalCount = 0;
                //char buffer[256] = { 0 };
                //int colorCount = sizeof(s.colorCounts) / sizeof(s.colorCounts[0]);

                //// todo fix this
                //int offset = snprintf(buffer, 256, "colors: ");
                //for (int i = 0; i < colorCount; ++i)
                //{
                //    offset += snprintf(buffer + offset, 256 - offset, "%d/", s.colorCounts[i]);
                //    totalCount += s.colorCounts[i];
                //}
                //snprintf(buffer + offset, 256 - offset, "[%d]", totalCount);
                //DrawTextLine(buffer);
                //DrawTextLine("stack allocator size = %d K", s.stackUsed / 1024);
                //DrawTextLine("total allocation = %d K", s.byteCount / 1024);
            }

            // Track maximum profile times
            {
                b2Profile p = B2Api.b2World_GetProfile(m_worldId);
                m_maxProfile.step = MathF.Max(m_maxProfile.step, p.step);
                m_maxProfile.pairs = MathF.Max(m_maxProfile.pairs, p.pairs);
                m_maxProfile.collide = MathF.Max(m_maxProfile.collide, p.collide);
                m_maxProfile.solve = MathF.Max(m_maxProfile.solve, p.solve);
                m_maxProfile.prepareStages = MathF.Max(m_maxProfile.prepareStages, p.prepareStages);
                m_maxProfile.solveConstraints = MathF.Max(m_maxProfile.solveConstraints, p.solveConstraints);
                m_maxProfile.prepareConstraints = MathF.Max(m_maxProfile.prepareConstraints, p.prepareConstraints);
                m_maxProfile.integrateVelocities = MathF.Max(m_maxProfile.integrateVelocities, p.integrateVelocities);
                m_maxProfile.warmStart = MathF.Max(m_maxProfile.warmStart, p.warmStart);
                m_maxProfile.solveImpulses = MathF.Max(m_maxProfile.solveImpulses, p.solveImpulses);
                m_maxProfile.integratePositions = MathF.Max(m_maxProfile.integratePositions, p.integratePositions);
                m_maxProfile.relaxImpulses = MathF.Max(m_maxProfile.relaxImpulses, p.relaxImpulses);
                m_maxProfile.applyRestitution = MathF.Max(m_maxProfile.applyRestitution, p.applyRestitution);
                m_maxProfile.storeImpulses = MathF.Max(m_maxProfile.storeImpulses, p.storeImpulses);
                m_maxProfile.transforms = MathF.Max(m_maxProfile.transforms, p.transforms);
                m_maxProfile.splitIslands = MathF.Max(m_maxProfile.splitIslands, p.splitIslands);
                m_maxProfile.jointEvents = MathF.Max(m_maxProfile.jointEvents, p.jointEvents);
                m_maxProfile.hitEvents = MathF.Max(m_maxProfile.hitEvents, p.hitEvents);
                m_maxProfile.refit = MathF.Max(m_maxProfile.refit, p.refit);
                m_maxProfile.bullets = MathF.Max(m_maxProfile.bullets, p.bullets);
                m_maxProfile.sleepIslands = MathF.Max(m_maxProfile.sleepIslands, p.sleepIslands);
                m_maxProfile.sensors = MathF.Max(m_maxProfile.sensors, p.sensors);

                m_totalProfile.step += p.step;
                m_totalProfile.pairs += p.pairs;
                m_totalProfile.collide += p.collide;
                m_totalProfile.solve += p.solve;
                m_totalProfile.prepareStages += p.prepareStages;
                m_totalProfile.solveConstraints += p.solveConstraints;
                m_totalProfile.prepareConstraints += p.prepareConstraints;
                m_totalProfile.integrateVelocities += p.integrateVelocities;
                m_totalProfile.warmStart += p.warmStart;
                m_totalProfile.solveImpulses += p.solveImpulses;
                m_totalProfile.integratePositions += p.integratePositions;
                m_totalProfile.relaxImpulses += p.relaxImpulses;
                m_totalProfile.applyRestitution += p.applyRestitution;
                m_totalProfile.storeImpulses += p.storeImpulses;
                m_totalProfile.transforms += p.transforms;
                m_totalProfile.splitIslands += p.splitIslands;
                m_totalProfile.jointEvents += p.jointEvents;
                m_totalProfile.hitEvents += p.hitEvents;
                m_totalProfile.refit += p.refit;
                m_totalProfile.bullets += p.bullets;
                m_totalProfile.sleepIslands += p.sleepIslands;
                m_totalProfile.sensors += p.sensors;
            }

            if (m_context.drawProfile)
            {
                b2Profile p = B2Api.b2World_GetProfile(m_worldId);

                b2Profile aveProfile = default;
                if (m_stepCount > 0)
                {
                    float scale = 1.0f / m_stepCount;
                    aveProfile.step = scale * m_totalProfile.step;
                    aveProfile.pairs = scale * m_totalProfile.pairs;
                    aveProfile.collide = scale * m_totalProfile.collide;
                    aveProfile.solve = scale * m_totalProfile.solve;
                    aveProfile.prepareStages = scale * m_totalProfile.prepareStages;
                    aveProfile.solveConstraints = scale * m_totalProfile.solveConstraints;
                    aveProfile.prepareConstraints = scale * m_totalProfile.prepareConstraints;
                    aveProfile.integrateVelocities = scale * m_totalProfile.integrateVelocities;
                    aveProfile.warmStart = scale * m_totalProfile.warmStart;
                    aveProfile.solveImpulses = scale * m_totalProfile.solveImpulses;
                    aveProfile.integratePositions = scale * m_totalProfile.integratePositions;
                    aveProfile.relaxImpulses = scale * m_totalProfile.relaxImpulses;
                    aveProfile.applyRestitution = scale * m_totalProfile.applyRestitution;
                    aveProfile.storeImpulses = scale * m_totalProfile.storeImpulses;
                    aveProfile.transforms = scale * m_totalProfile.transforms;
                    aveProfile.splitIslands = scale * m_totalProfile.splitIslands;
                    aveProfile.jointEvents = scale * m_totalProfile.jointEvents;
                    aveProfile.hitEvents = scale * m_totalProfile.hitEvents;
                    aveProfile.refit = scale * m_totalProfile.refit;
                    aveProfile.bullets = scale * m_totalProfile.bullets;
                    aveProfile.sleepIslands = scale * m_totalProfile.sleepIslands;
                    aveProfile.sensors = scale * m_totalProfile.sensors;
                }

                //DrawTextLine("step [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.step, aveProfile.step, m_maxProfile.step);
                //DrawTextLine("pairs [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.pairs, aveProfile.pairs, m_maxProfile.pairs);
                //DrawTextLine("collide [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.collide, aveProfile.collide, m_maxProfile.collide);
                //DrawTextLine("solve [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.solve, aveProfile.solve, m_maxProfile.solve);
                //DrawTextLine("> merge islands [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.mergeIslands, aveProfile.mergeIslands,
                //              m_maxProfile.mergeIslands);
                //DrawTextLine("> prepare tasks [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.prepareStages, aveProfile.prepareStages,
                //              m_maxProfile.prepareStages);
                //DrawTextLine("> solve constraints [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.solveConstraints, aveProfile.solveConstraints,
                //              m_maxProfile.solveConstraints);
                //DrawTextLine(">> prepare constraints [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.prepareConstraints,
                //              aveProfile.prepareConstraints, m_maxProfile.prepareConstraints);
                //DrawTextLine(">> integrate velocities [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.integrateVelocities,
                //              aveProfile.integrateVelocities, m_maxProfile.integrateVelocities);
                //DrawTextLine(">> warm start [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.warmStart, aveProfile.warmStart,
                //              m_maxProfile.warmStart);
                //DrawTextLine(">> solve impulses [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.solveImpulses, aveProfile.solveImpulses,
                //              m_maxProfile.solveImpulses);
                //DrawTextLine(">> integrate positions [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.integratePositions,
                //              aveProfile.integratePositions, m_maxProfile.integratePositions);
                //DrawTextLine(">> relax impulses [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.relaxImpulses, aveProfile.relaxImpulses,
                //              m_maxProfile.relaxImpulses);
                //DrawTextLine(">> apply restitution [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.applyRestitution, aveProfile.applyRestitution,
                //              m_maxProfile.applyRestitution);
                //DrawTextLine(">> store impulses [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.storeImpulses, aveProfile.storeImpulses,
                //              m_maxProfile.storeImpulses);
                //DrawTextLine(">> split islands [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.splitIslands, aveProfile.splitIslands,
                //              m_maxProfile.splitIslands);
                //DrawTextLine("> update transforms [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.transforms, aveProfile.transforms,
                //              m_maxProfile.transforms);
                //DrawTextLine("> joint events [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.jointEvents, aveProfile.jointEvents,
                //              m_maxProfile.jointEvents);
                //DrawTextLine("> hit events [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.hitEvents, aveProfile.hitEvents,
                //              m_maxProfile.hitEvents);
                //DrawTextLine("> refit BVH [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.refit, aveProfile.refit, m_maxProfile.refit);
                //DrawTextLine("> sleep islands [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.sleepIslands, aveProfile.sleepIslands,
                //              m_maxProfile.sleepIslands);
                //DrawTextLine("> bullets [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.bullets, aveProfile.bullets, m_maxProfile.bullets);
                //DrawTextLine("sensors [ave] (max) = %5.2f [%6.2f] (%6.2f)", p.sensors, aveProfile.sensors, m_maxProfile.sensors);
            }
        }

        public virtual void UpdateGui()
        {
        }

        public virtual void Keyboard(int key)
        {
        }

        public virtual void MouseLeftButtonDown(Vector2 p,  int button, int mod)
        {
            if (m_mouseJointId.index1 == 0)
            {
                return;
            }

            //if (button == GLFW_MOUSE_BUTTON_1)
            //{
            //    // Make a small box.
            //    b2AABB box;
            //    b2Vec2 d = { 0.001f, 0.001f };
            //    box.lowerBound = b2Sub(p, d);
            //    box.upperBound = b2Add(p, d);

            //    // Query the world for overlapping shapes.
            //    QueryContext queryContext = { p, b2_nullBodyId };
            //    b2World_OverlapAABB(m_worldId, box, b2DefaultQueryFilter(), QueryCallback, &queryContext);

            //    if (B2_IS_NON_NULL(queryContext.bodyId))
            //    {
            //        b2BodyDef bodyDef = b2DefaultBodyDef();
            //        m_groundBodyId = b2CreateBody(m_worldId, &bodyDef);

            //        b2MouseJointDef jointDef = b2DefaultMouseJointDef();
            //        jointDef.base.bodyIdA = m_groundBodyId;
            //        jointDef.base.bodyIdB = queryContext.bodyId;
            //        jointDef.base.localFrameA.p = p;
            //        jointDef.base.localFrameB.p = b2Body_GetLocalPoint(queryContext.bodyId, p);
            //        jointDef.hertz = 7.5f;
            //        jointDef.dampingRatio = 0.7f;
            //        jointDef.maxForce = 1000.0f * b2Body_GetMass(queryContext.bodyId) * b2Length(b2World_GetGravity(m_worldId));
            //        m_mouseJointId = b2CreateMouseJoint(m_worldId, &jointDef);

            //        b2Body_SetAwake(queryContext.bodyId, true);
            //    }
            //}
        }

        public virtual void MouseUp(Vector2 p, int button)
        {
            //if (b2Joint_IsValid(m_mouseJointId) == false)
            //{
            //    // The world or attached body was destroyed.
            //    m_mouseJointId = b2_nullJointId;
            //}

            //if (B2_IS_NON_NULL(m_mouseJointId) && button == GLFW_MOUSE_BUTTON_1)
            //{
            //    b2DestroyJoint(m_mouseJointId);
            //    m_mouseJointId = b2_nullJointId;

            //    b2DestroyBody(m_groundBodyId);
            //    m_groundBodyId = b2_nullBodyId;
            //}
        }

        public virtual void MouseMove(Vector2 p)
        {
            //if (b2Joint_IsValid(m_mouseJointId) == false)
            //{
            //    // The world or attached body was destroyed.
            //    m_mouseJointId = b2_nullJointId;
            //}

            //if (B2_IS_NON_NULL(m_mouseJointId))
            //{
            //    b2Transform localFrameA = { p, b2Rot_identity };
            //    b2Joint_SetLocalFrameA(m_mouseJointId, localFrameA);
            //    b2BodyId bodyIdB = b2Joint_GetBodyB(m_mouseJointId);
            //    b2Body_SetAwake(bodyIdB, true);
            //}
        }

        public void DrawTextLine(params string[] text)
        {
            //va_list arg;
            //va_start(arg, text);
            //ImGui::Begin("Overlay", nullptr,
            //    ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoInputs | ImGuiWindowFlags_AlwaysAutoResize |
            //    ImGuiWindowFlags_NoScrollbar);
            //ImGui::PushFont(m_context->draw.m_regularFont);
            //ImGui::SetCursorPos(ImVec2(5.0f, float(m_textLine)));
            //ImGui::TextColoredV(ImColor(230, 153, 153, 255), text, arg);
            //ImGui::PopFont();
            //ImGui::End();
            //va_end(arg);

            m_textLine += m_textIncrement;
        }

        public void ResetProfile()
        {
            m_totalProfile = default;
            m_maxProfile = default;
            m_stepCount = 0;
        }

        public void ShiftOrigin(Vector2 newOrigin)
        { }

        /// <summary>
        /// Parse an SVG path element with only straight lines. Example:
        /// "M 47.625004,185.20833 H 161.39585 l 29.10417,-2.64583 26.45834,-7.9375 26.45833,-13.22917 23.81251,-21.16666 h "
        /// "13.22916 v 44.97916 H 592.66669 V 0 h 21.16671 v 206.375 l -566.208398,-1e-5 z"
        /// </summary>
        public static int ParsePath(string svgPath, Vector2 offset, Vector2[] points, int capacity, float scale, bool reverseOrder)
        {
            int pointCount = 0;

            //todo might wanna replace this with an existing C# implementation.

            //b2Vec2 currentPoint = { };
            //const char* ptr = svgPath;
            //char command = *ptr;

            //while (*ptr != '\0')
            //{
            //    if (isdigit(*ptr) == 0 && *ptr != '-')
            //    {
            //        // note: command can be implicitly repeated
            //        command = *ptr;

            //        if (command == 'M' || command == 'L' || command == 'H' || command == 'V' || command == 'm' || command == 'l' ||
            //             command == 'h' || command == 'v')
            //        {
            //            ptr += 2; // Skip the command character and space
            //        }

            //        if (command == 'z')
            //        {
            //            break;
            //        }
            //    }

            //    assert(isdigit(*ptr) != 0 || *ptr == '-');

            //    float x = 0.0f, y = 0.0f;
            //    switch (command)
            //    {
            //        case 'M':
            //        case 'L':
            //            if (sscanf(ptr, "%f,%f", &x, &y) == 2)
            //            {
            //                currentPoint.x = x;
            //                currentPoint.y = y;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;
            //        case 'H':
            //            if (sscanf(ptr, "%f", &x) == 1)
            //            {
            //                currentPoint.x = x;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;
            //        case 'V':
            //            if (sscanf(ptr, "%f", &y) == 1)
            //            {
            //                currentPoint.y = y;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;
            //        case 'm':
            //        case 'l':
            //            if (sscanf(ptr, "%f,%f", &x, &y) == 2)
            //            {
            //                currentPoint.x += x;
            //                currentPoint.y += y;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;
            //        case 'h':
            //            if (sscanf(ptr, "%f", &x) == 1)
            //            {
            //                currentPoint.x += x;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;
            //        case 'v':
            //            if (sscanf(ptr, "%f", &y) == 1)
            //            {
            //                currentPoint.y += y;
            //            }
            //            else
            //            {
            //                assert(false);
            //            }
            //            break;

            //        default:
            //            assert(false);
            //            break;
            //    }

            //    points[pointCount] = { scale * (currentPoint.x + offset.x), -scale * (currentPoint.y + offset.y) }
            //    ;
            //    pointCount += 1;
            //    if (pointCount == capacity)
            //    {
            //        break;
            //    }

            //    // Move to the next space or end of string
            //    while (*ptr != '\0' && isspace(*ptr) == 0)
            //    {
            //        ptr++;
            //    }

            //    // Skip contiguous spaces
            //    while (isspace(*ptr))
            //    {
            //        ptr++;
            //    }

            //    ptr += 0;
            //}

            //if (pointCount == 0)
            //{
            //    return 0;
            //}

            //if (reverseOrder)
            //{
            //}
            return pointCount;
        }

        //friend class DestructionListener;
        //friend class BoundaryListener;
        //friend class ContactListener;

        public const int m_maxTasks = 64;
        public const int m_maxThreads = 64;

#if DEBUG
        public const bool m_isDebug = true;    
#else
        public const bool m_isDebug = false;
#endif

        public SampleContext m_context;

        // enki::TaskScheduler* m_scheduler;
        // class SampleTask* m_tasks;
        // public int m_taskCount;
        public int m_threadCount;

        public b2BodyId m_groundBodyId;

        public b2WorldId m_worldId;
        public b2JointId m_mouseJointId;
        public int m_stepCount;
        public b2Profile m_maxProfile;
        public b2Profile m_totalProfile;

        private int m_textLine;
        private int m_textIncrement;

        
    }
}
