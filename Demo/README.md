# Demo

This folder should contain code to achieve a working demo of taking an Android App and a binding library, referencing Android Support libraries and jetifying, cecilfying, and doing whatever is necessary to create a demo to show the app running afte the migration.

The goal is to create a demo very quickly.  This will require creating custom build tasks to perform various actions during different times of the build process.  We are not looking for reusibility or efficiency in code, but primarily function.  This could involve executing arbitrary scripts, custom tasks, etc which just stitch things together enough to run.

We want a demo to prove this can work and that the experience is reasonable.  We want to see what the debug experience looks like.

To get a better idea of the build process and where we need to inject ourselves see this doc: https://paper.dropbox.com/doc/AndroidX-Migration-Plan--AYadDNtXU5xOyiW13CyymVccAg-h3CswlPGJitJp0rx6gcI1#:uid=132759554584123419009628&h2=Build-Process-(aapt2)
