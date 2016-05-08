/// <binding BeforeBuild='build' />
var gulp = require('gulp');
var util = require('gulp-util');
var plumber = require('gulp-plumber');
var sourcemaps = require('gulp-sourcemaps');
var minifyHtml = require('gulp-html-minifier');
var webpack = require('webpack');
var webpackStream = require('webpack-stream');
var cssNano = require('gulp-cssnano');
var webserver = require('gulp-webserver');
var exec = require('child_process').exec;
var path = require('path');

var debug = (process.env.NODE_ENV !== 'production');

var config = {
    paths: {
        allSrc: './src/**/*',
        html: './src/*.html',
        css: './src/*.css',
        js: './src/**/*.js',
        mainJs: './src/main.ts',
        dist: '../..'
    }
};

var uglifyJs = new webpack.optimize.UglifyJsPlugin({ comments: /a^/ });
var defines = new webpack.DefinePlugin({ DEBUG: debug });

var webpackConfig = {
    entry: config.paths.mainJs,
    output: {
        filename: 'app.js'
    },
    resolve: {
        extensions: ['', '.webpack.js', '.web.js', '.ts']
    },
    module: {
        loaders: [
            {
                test: /\.ts$/,
                exclude: /node_modules/,
                loader: 'babel-loader!eslint-loader!ts-loader'
            }
        ]
    },
    plugins: debug ? [defines] : [defines, uglifyJs],
    devtool: debug ? '#cheap-module-eval-source-map' : undefined
};

gulp.task('css', function () {
    return gulp.src(config.paths.css)
        .pipe(plumber())
        .pipe(debug ? sourcemaps.init() : util.noop())
        .pipe(cssNano())
        .pipe(debug ? sourcemaps.write() : util.noop())
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('js', function () {
    return gulp.src(config.paths.js)
        .pipe(plumber())
        .pipe(webpackStream(webpackConfig, webpack))
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('html', function () {
    return gulp.src(config.paths.html)
        .pipe(plumber())
        .pipe(minifyHtml({
            removeComments: true, collapseWhitespace: true, lint: true, keepClosingSlash: true, minifyJS: true
        }))
        .pipe(gulp.dest(config.paths.dist));
});

gulp.task('generate', function (done) {
    exec('src\\Win32ErrorTable\\bin\\Debug\\Win32ErrorTable.exe', { cwd: path.resolve('../..') }, function (err, stdout, stderr) {
        console.log(stdout);
        if (err) {
            console.log(stderr);
            done(err);
        } else {
            done();
        }
    });
});

gulp.task('build', ['html', 'css', 'js', 'generate']);

gulp.task('watch', ['build'], function () {
    gulp.watch(config.paths.allSrc, ['build']);
});

gulp.task('serve', ['build'], function() {
    gulp.src('../..')
        .pipe(webserver({
            livereload: true,
            open: true
        }));
});

gulp.task('default', debug ? ['watch', 'serve'] : ['build']);