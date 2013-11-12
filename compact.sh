#!/bin/sh

set -e

cd $(notmuch config get database.path)/.notmuch
xapian-compact --no-renumber xapian xapian-compacted

rm -R xapian
mv xapian-compacted xapian

