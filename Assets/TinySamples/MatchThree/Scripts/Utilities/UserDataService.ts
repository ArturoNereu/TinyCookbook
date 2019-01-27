
namespace game {

    /**
     * Save and load player data into web cookies.
     */
    export class UserDataService {
        
        static getBestScore(levelID: number): number {
            let cookieName = "BestScore" + levelID;
            let value = this.getCookie(cookieName);
            if (value) {
                return Number(value);
            }
            else {
                return 0;
            }
        }

        static setBestScore(levelID: number, score: number): void {
            let cookieName = "BestScore" + levelID;
            this.setCookie(cookieName, String(score));
        }

        static getSelectedWorldMapIndex(): number {
            let cookieName = "SelectedWorldMapIndex";
            let value = this.getCookie(cookieName);
            if (value) {
                return Number(value);
            }
            else {
                return 0;
            }
        }

        static setSelectedWorldMapIndex(itemIndex: number): void {
            let cookieName = "SelectedWorldMapIndex";
            this.setCookie(cookieName, String(itemIndex));
        }

        static getLastBeatenLevelID(): number {
            let cookieName = "LastBeatenLevelID";
            let value = this.getCookie(cookieName);
            if (value) {
                return Number(value);
            }
            else {
                return 0;
            }
        }

        static setLastBeatenLevelID(levelID: number): void {
            let cookieName = "LastBeatenLevelID";
            this.setCookie(cookieName, String(levelID));
        }

        static getIsSoundOn(): boolean {
            let cookieName = "Sound";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return true;
            }
        }

        static setIsSoundOn(isSoundOn: boolean): void {
            let cookieName = "Sound";
            this.setCookie(cookieName, String(isSoundOn ? 1 : 0));
        }

        static getIsMusicOn(): boolean {
            let cookieName = "Music";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return true;
            }
        }

        static setIsMusicOn(isMusicOn: boolean): void {
            let cookieName = "Music";
            this.setCookie(cookieName, String(isMusicOn ? 1 : 0));
        }

        static getLanguageID(): string {
            let cookieName = "LanguageID";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName);
            }
            else {
                return "en";
            }
        }

        static setLanguageID(languageID: string): void {
            let cookieName = "LanguageID";
            this.setCookie(cookieName, languageID);
        }

        static getHasSeenCutscene(): boolean {
            let cookieName = "CutsceneSeen";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return false;
            }
        }

        static setHasSeenCutscene(hasSeen: boolean): void {
            let cookieName = "CutsceneSeen";
            this.setCookie(cookieName, String(hasSeen ? 1 : 0));
        }

        static getHasSeenEndCutscene(): boolean {
            let cookieName = "EndCutsceneSeen";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return false;
            }
        }

        static setHasSeenEndCutscene(hasSeen: boolean): void {
            let cookieName = "EndCutsceneSeen";
            this.setCookie(cookieName, String(hasSeen ? 1 : 0));
        }

        static getIsMatchTutorialDone(): boolean {
            let cookieName = "MatchTutorialDone";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return false;
            }
        }

        static setIsMatchTutorialDone(value: boolean): void {
            let cookieName = "MatchTutorialDone";
            this.setCookie(cookieName, String(value ? 1 : 0));
        }

        static getIsEggTutorialDone(): boolean {
            let cookieName = "EggTutorialDone";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return false;
            }
        }

        static setIsEggTutorialDone(value: boolean): void {
            let cookieName = "EggTutorialDone";
            this.setCookie(cookieName, String(value ? 1 : 0));
        }

        static getIsSurvivalTutorialDone(): boolean {
            let cookieName = "SurvivalTutorialDone";
            let value = this.getCookie(cookieName);
            if (value) {
                return this.getCookie(cookieName) == "1" ? true : false;
            }
            else {
                return false; 
            }
        }

        static setIsSurvivalTutorialDone(value: boolean): void {
            let cookieName = "SurvivalTutorialDone";
            this.setCookie(cookieName, String(value ? 1 : 0));
        }
        

        



        static getCookie(name: string) {
            const value = "; " + document.cookie;
            const parts = value.split("; " + name + "=");
            if (parts.length == 2) {
                return parts.pop().split(";").shift();
            }
        }

        static setCookie(name: string, val: string) {
            const date = new Date();
            const value = val;
            date.setTime(date.getTime() + (1000 * 24 * 60 * 60 * 1000));
            document.cookie = name + "=" + value + "; expires=" + date.toUTCString() + "; path=/";
        }
        
        static deleteCookie(name: string) {
            const date = new Date();
            date.setTime(date.getTime() + (-1 * 24 * 60 * 60 * 1000));
            document.cookie = name + "=; expires=" + date.toUTCString() + "; path=/";
        }

        static deleteAllCookies() {
            var cookies = document.cookie.split(";");
            for (var i = 0; i < cookies.length; i++) {
                var cookie = cookies[i];
                var eqPos = cookie.indexOf("=");
                var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
                document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
            }
        }
    }
}
