
# coding: utf-8

# # Problem C

# Design an efficient implementation of Random Oracle hash function of your own with 32 bit long output. Can you produce a collision, i.e., find two distinct messages x_0, x_1 such that h(x_0) = h (x_1).

# In[57]:

import random

class random_oracle_hash(object):

    # dictionnary to store already seen inputs
    map_table = dict()
    
    def get_key_from_hash(self, h):
        # get the entries with hash h
        values = [key for key, value in self.map_table.items() if value == h]
        return values
    
    def get_all_hashes(self):
        # return all stored hashes
        return self.map_table.values()

    def get_hash(self, s):
        # check if entry already exists
        if s in self.map_table:
            return self.map_table[s]

        # assign random integer between 0 and 2^32-1, these values can all be represented in 32 bits
        random_value = random.randint(0,2**32-1)
        # integer to 32-bits format
        h = "{:032b}".format(random_value)
        # add entry to dictionnary
        self.map_table[s] = h

        #return hash
        return h


# In[59]:

def find_collision():
    # init oracle object
    ROH = random_oracle_hash()
            
    collision_found = False
    
    iteration_n = 1
    # hash integer until a collision is found
    while not collision_found:
        
        # prints every 10000 iteration
        if iteration_n % 10000 == 0:
            print('iteration {}'.format(iteration_n))
        
        # all hashes before add entry
        hashes_n = [x for x in ROH.get_all_hashes()]
        
        # add new entry
        hash_n = ROH.get_hash(iteration_n)
        
        # checks if entry is already in dictionnary
        if hash_n in hashes_n:
            collision_found = True
            print('collision found for hash {} at iteration {}'.format(hash_n, iteration_n))
            print('for entries {}'.format(ROH.get_key_from_hash(hash_n)))
        # if not, continues
        else:
            iteration_n += 1


# In[60]:

find_collision()

