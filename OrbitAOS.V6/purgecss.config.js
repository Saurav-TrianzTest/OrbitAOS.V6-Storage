module.exports = {
  content: [
    './Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
    './wwwroot/css/**/*.css'
  ],
  css: [
    './wwwroot/lib/bootstrap/dist/css/bootstrap.css',
    './wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.css',
    './wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.css',
    './wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.css'
  ],
  output: './wwwroot/lib/bootstrap/dist/css/purged/'
};
