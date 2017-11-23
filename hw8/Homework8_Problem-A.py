#! /usr/bin/python
# coding: utf-8

# imports
from hashlib import sha256
from multiprocessing import Pool
import random
from time import time


MESSAGE = "BBB"  # Set which message to hash
DIFFICULTY = 5  # Set how many 0 (zeros) the hash should start with
message_encoded = MESSAGE.encode()
startswith_string = "0"*DIFFICULTY


def hash_str(s, nonce):
    """
    return: The sha256 hash of the string concatenated with the nonce.
    """
    nonce = str(nonce)
    s = str(s)
    return sha256((s+nonce).encode()).hexdigest()

def check(nonce):
    """
    Returns True only if the MESSAGE and the given nonce generate a sha256 hash starting with DIFFICULTY zeros.
    """
    return sha256(message_encoded+nonce.encode()).hexdigest()[:DIFFICULTY] == startswith_string


def check_range(from_, to, check_fun):
    """
    Executes the check_fun with each integer value in the interval [from_, to). 
    Note that 'to' is not included.
    :param from_: int, included
    :param to: int, not included
    :param check_fun: Function taking one string argument and returning a boolean.
    :return the first value for which the check_fun returns True. If no such value was found, returns None
    """
    for n in range(from_, to):
        if check_fun(str(n)):
            return n
    return None

def check_until_found(check_fun, from_=0, random_=None):
    """
    Executes the check_fun until the check_fun returns True.
    If random_ is given, checks values between 0 and 10**100 randomly (with random_ as seed).
    If not, checks values starting from from_ (default 0).
    
    :param check_fun: Function taking one string argument and returning a boolean.
    :param from_: int, where to start
    :param random: if not None, then from_ is ignored, and the nonces are checked randomly with random_ as seed
    :return the first value for which the check_fun returns True.
    """
    
    assert isinstance(from_, int) and from_ >= 0
    assert random_ is None or isinstance(random_, int)
    
    if random_ is not None:
        print("check until found randomly (seed: {})".format(random_))
        random.seed(random_)
        _max_random = 10**100
        while True:
            n = random.randint(0, _max_random)
            if check_fun(str(n)):
                return n
        
    else:
        print("check until found linear (starting at {})".format(from_))
        n=from_
        while True:
            if check_fun(str(n)):
                return n
            n += 1
    

# ###################### MAIN #################

if __name__ == "__main__":

    # ## Mining

    # Find the smallest nonce possible
    nonce = check_until_found(check_fun=check)
    print("Found nonce: ", nonce, "resulting in hash:", hash_str("BBB", nonce=nonce))


    # ## Multiprocessor Variant
    # Try random values using multiple, parallel processes.
    # Terminates the search after the first nonce is found.

    NBR_SEARCHES = 3  # How many processes to use.
    pool = Pool(NBR_SEARCHES)
    start_t = time()
    def async_callback(result):
        h = hash_str(MESSAGE, result)
        print("Found nonce {} resulting in the hash {}".format(result, h))
        print("-----------------------------------------------------")
        t = time()
        print("Search took {} seconds".format(t-start_t))
        print("Terminating pool...", end='')
        pool.close()
        pool.terminate()
        print("done")

    answer_objs = []
    for k in range(NBR_SEARCHES):
        answer_obj = pool.apply_async(check_until_found, args=(check, 0, random.randint(0, 10**10)), callback=async_callback, error_callback=lambda err: print("There was an error: ", err))
        answer_objs.append(answer_obj)
    
    pool.close()
    pool.join()


# ### Some noces we found:
# 
# #### Difficulty 5:
# Takes almost no time. Max 2 seconds
# - 452524 (This is the smallest nonce for this difficulty)
# - 448920218221668008890177062840934330544926360737972523265869865846080527648390579977647209038784885
# - 5116460947447678134242415639319596110927307992229534935518086940925724834098545044804512960047541820
# 
# #### Difficulty 6:
# Normally takes not more than 15 seconds (using 3 cores)
# - 65940717 (This is the smallest nonce for this difficulty)
# - 8675333746398227032255764553342247556395403899377430691196750306254353085969357040256482472627797848
# - 4413630770167078031659890693872953162325083177511466205542441707732338104971993113505158293900532682
# - 5152273404238328313184681473108899257818981175352338754761401479660692696327989293448925898198808685
# 
# #### Difficulty 7:
# Takes around 5 minutes (using 3 cores)
# - 3734805048231636558000882470270270562362373618415981692215899228998575479488680089323266160261234073 (Interestingly, this nonce was found during the search for difficulty 6)
# - 6839743781002907121767199644378149046565371769144013797711135188157298083245853823815810267363131859
# - 371910028096327238548271594327435482125513937170959974809188460980794286980070817604192638247128517
