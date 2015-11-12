/// <binding AfterBuild='copy' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    rimraf = require('rimraf'),
    fs = require('fs');

eval('var project = ' + fs.readFileSync('./project.json'));

gulp.task('copy', ['clean'], function () {
    var bower = {
        "bootstrap": "bootstrap/dist/**/*.{js,map,css,ttf,svg,woff,eot}"
    }

    for (var destinationDir in bower)
    {
        gulp.src('./bower_components/' + bower.bootstrap).pipe(gulp.dest('./' + project.webroot + '/lib/' + destinationDir))
    }
});

gulp.task('clean', function (callback) {
    rimraf('./' + project.webroot + '/lib/', callback)
});