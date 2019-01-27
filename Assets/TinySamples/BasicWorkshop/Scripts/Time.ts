namespace ut
{
    /**
     * Placeholder system to provide a UnityEngine.Time like API
     *
     * e.g. 
     *  let deltaTime = ut.Time.deltaTime();
     *  let time = ut.Time.time();
     *
     */
    @ut.executeBefore(ut.Shared.UserCodeStart)
    export class Time extends ut.ComponentSystem
    {
        private static _deltaTime: number = 0;
        private static _time: number = 0;
        
        static deltaTime(): number {
            return Time._deltaTime;
        }

        static time(): number {
            return Time._time;
        }

        static reset()
        {
           Time._time = 0;
        }
        
        OnUpdate(): void {
            let dt = this.scheduler.deltaTime();
            Time._deltaTime = dt;
            Time._time += dt;
        }
    }
}
