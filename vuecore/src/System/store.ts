import Vue from "vue";
import * as moment from "moment";
import { StorageService } from "./localstorage";
import * as cl from "../../code/Backend/repo/t4/hubsflow";
import Vuex from "vuex";
Vue.use(Vuex);
interface Oauth {
  data: object;
  provider: string;
}
export interface storeData {
  count: number;
  isAuth: boolean;
  lang: string;
  mandantid: number;
  location: string;
  token: string;
  servurl: string;
  dateformat: string;
  oauth: Oauth;
}
var host =
  window.document.location.port == "8080"
    ? "http://localhost:5000"
    : window.location.origin;
//will be in local storage
const dstate: storeData = {
  count: 0,
  isAuth: false,
  token: "",
  lang: "de",
  mandantid: 0,
  location: "AT",
  servurl: host,
  dateformat: "DD.MM.YYYY",
  oauth: null
  //db : null
};

const storage = new StorageService();
//set initial state in localstorage if doesnt exist
storage.setItemInit(storage.C_ENV_KEY, dstate);

const storeData: storeData = JSON.parse(storage.getItem(storage.C_ENV_KEY));

//defining state
export interface State {
  db: cl.SgnRCloud;
  vars: storeData;
}
//you can have multiple slots in state, local state and imported types
const statee: State = {
  db: new cl.SgnRCloud(dstate.servurl, storeData.token),
  vars: storeData
};
//creating typed vuex
const store = new Vuex.Store({
  state: statee,
  mutations: {
    setvars(state, s: storeData) {
      state.vars = s;
      storage.setItem(storage.C_ENV_KEY, s);
    },
    setdb(state, s: cl.SgnRCloud) {
      state.db = s;
    }
  }
});

store.subscribe((mutate, statee) => {
  if (mutate.type == "setvars") {
    console.log("subscribed muttate");
  }
});

export default store;
