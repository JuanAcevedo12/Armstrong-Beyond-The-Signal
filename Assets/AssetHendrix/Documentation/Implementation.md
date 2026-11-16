# CARONTE prototype

Play scene Assets/Scenes/Prototipo.unity. All created assets belong to Assets/AssetHendrix.

Visual: X width 2, Y height 3, Z depth 1; payload faces -Z. Visual dimensions are enlarged didactic proportions; NASA structure reference 100 x 226.3 x 366 mm, https://www.nasa.gov/smallsat-institute/sst-soa/structures-materials-and-mechanisms/ .

Replace the prefabs assigned to CarontePrototype arrays, keeping centered local origin and anchor orientation. Technical properties live only in MissionConfig, independent of meshes. Left panel extends along -X; right anchor rotates 180 degrees.

Simulation: deterministic 30 s steps for 30 days; circular Kepler period with mu=398600.4418 km3/s2, R=6371 km. Eclipse fraction 36%, 4 or 5 eight-minute daily contacts are didactic assumptions, not propagated ground visibility. Solar generation, payload, mass, budget and communication values are didactic. MB decimal, Mbps / 8 for MB/s. Storage cannot become negative; conservation captured = sent + stored + lost. No NASA launch reliability claims.

HTML retained: category catalog, immediate resource checks, configuration JSON, engineering console. Modified: 6U, 30-day discrete simulation, deterministic resource failures, native 3D prefabs. Deferred: interplanetary missions, VR, multiplayer. This is an initial implementation; orbital contact geometry, advanced ADCS/thermal models and polished transitions remain pending.
