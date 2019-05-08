# Mighty Docs

Install Ruby.

On Windows:

 - v2.5 works, v2.6 is available but incomplete [as of April 2019]
 - You do need the base install of MSYS2 which you will be offered with the installer
   - (This is a requirement for certain C++ - based Ruby gems on Windows)

Then, from the command prompt:

`> gem install jekyll`
`> gem install bundler`

Then, inside this `docs` directory:

`> bundle install`

That is all the one-off installation tasks.

Then, to build and serve the docs locally, from this `docs` directory:

`> bundle exec jekyll serve`

and browse to http://localhost:4000/
