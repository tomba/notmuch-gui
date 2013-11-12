#!/bin/bash

mails=(`notmuch config get user.primary_email`)
mails+=(`notmuch config get user.other_email`)

to=(${mails[@]})
to=(${to[@]/#/to:})
to=${to[@]}
to=${to// / OR }
to="($to)"

from=(${mails[@]})
from=(${from[@]/#/from:})
from=${from[@]}
from=${from// / OR }
from="($from)"

#set -x

new="tag:new AND "
new=

notmuch tag -inbox +to-me -- $new $to

notmuch tag -inbox +from-me -- $new $from

notmuch tag -inbox +linux-kernel -- $new to:linux-kernel@vger.kernel.org
notmuch tag -inbox +linux-omap -- $new to:linux-omap@vger.kernel.org
notmuch tag -inbox +linux-fbdev -- $new to:linux-fbdev@vger.kernel.org
notmuch tag -inbox +linux-arm -- $new to:linux-arm-kernel@lists.infradead.org

notmuch tag -inbox +users-kernel-org -- $new to:users@linux.kernel.org

notmuch tag -new -- tag:new

