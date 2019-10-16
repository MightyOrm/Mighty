# Mighty Docs

Click this button to read Mighty's:

[![Documentation](https://img.shields.io/badge/-Documentation-f52a62.svg)](https://mightyorm.github.io/Mighty/)


What follows are instructions for building a local copy of the Mighty docs.

Install Ruby.

On Windows:

 - v2.5 works, v2.6 is available but incomplete [as of April 2019]
 - You need the base install of MSYS2 which you will be offered with the installer
   - (This is a requirement for certain C++ - based Ruby gems on Windows)

Then, from the command prompt:

`> gem install jekyll`
`> gem install bundler`

Then, inside this `docs` directory:

`> bundle install`

That completes all the one-off installation tasks.

After that, to test the docs locally, from this `docs` directory:

`> bundle exec jekyll serve`

and browse to http://localhost:4000/ .