#!/bin/sh

MONO_PREFIX=/opt/monodevelop
export LD_LIBRARY_PATH=$MONO_PREFIX/lib:$LD_LIBRARY_PATH
export PKG_CONFIG_PATH=$MONO_PREFIX/lib/pkgconfig
export PATH=$MONO_PREFIX/bin:$PATH
export MONO_GAC_PREFIX=/opt/monodevelop:/usr
export debian_chroot=mono

bash

