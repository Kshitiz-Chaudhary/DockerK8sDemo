var app = new Vue({
  el: '#app',
  data: {
    id_url: 'http://localhost:8088',
    api_url: 'http://localhost:8080',
    username: 'admin',
    acc_username: 'admin',
    password: 'Qwerty!23',
    status: 'logged in',
    token: 'eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ7XCJJZFwiOlwiM2E3NTAwOTAtZTQ0NS00ZjE5LTlkMWItMzA1OTM0MDE1NmM0XCIsXCJVc2VybmFtZVwiOlwiYWRtaW5cIn0iLCJqdGkiOiIzYTc1MDA5MC1lNDQ1LTRmMTktOWQxYi0zMDU5MzQwMTU2YzQiLCJpYXQiOiIwNy8xNS8yMDE4IDE4OjM4OjA4Iiwia2V5aWQiOiIxNTMwNjMzMjA1Iiwicm9sZXMiOiJbXCJhZG1pblwiLFwiY29udHJpYnV0b3JcIixcInJlYWRlclwiXSIsIm5iZiI6MTUzMTY3OTg4OCwiZXhwIjoxNTMxODUyNjg4LCJpc3MiOiJleGFtcGxlLmNvbSIsImF1ZCI6ImV4YW1wbGUuY29tIn0.IUxvcuoFYpm0Q38XKDSSIBXhGLDkQYxqpl6GQzDYCH7pZ_1kcBR0MpIWUMbghIKwZTVcAcojrJImqimGKhiQhiVe3AsH06ukM9CXId--x_K2hJJCUroJwflei02P751Pdf_yXKUzS-t-qhQxBL-cQl849iLBRs11_4zBTRiZ_-oHVjD4IPBor1hJhgRNJO8Mx3Hrq-Q2BBXM0tuk6EWv6dukQGSkd8itBhTbv4uDzF4d37lxnP7aALzzQ3YPWwTc57ItQTWOxI_F3yPtp7-wwQbPzxS8LG9I_JOmgjJTW7qbk_5kYldiuqbOaueIzF-hVUQ44FFY9oHVDB2BrTvgPg',
    account: ''
  },
  methods: {
    login: function () {
      var url = this.id_url + "/token?sigType=asymmetric";
      var apiKey = btoa(this.username+":"+this.password);
      console.log("login " + this.username + ":" + this.password + " to " + url + ", key " + apiKey);

      axios.get(url, {
        baseURL: url,
        timeout: 1000,
        headers: {'Authorization': 'Basic ' + apiKey}
      }).then(response => {
        console.log("token received: " + response.data);
        this.token = response.data;
        this.status = 'logged in';
      }, response => {
        console.log("error: " + response.data);
        this.token = 'none';
        this.status = 'login failed - ' + response.data;
      });
    },
    getAccount: function () {
      var url = this.api_url + "/user/" + this.acc_username;
      console.log("getting account info " + url);

      axios.get(url, {
        baseURL: url,
        timeout: 1000,
        headers: {'Authorization': 'Bearer ' + this.token}
      }).then(response => {
        console.log("success received: " + response.data);
        this.account = response.data; 
      }, response => {
        console.log("error: " + response.data);
        this.account = '';
      });
    }
  }
});