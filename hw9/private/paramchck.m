function [errorcode,varargout] = paramchck(nparms,varargin)
%PARAMCHCK Checks the argument list for the probability functions.

errorcode = 0;
varargout = varargin;

if nparms == 1
    return;
end

% Get size of each input, check for scalars, copy to output
isscalar = (cellfun('prodofsize',varargin) == 1);

% Done if all inputs are scalars.  Otherwise fetch their common size.
if (all(isscalar)), return; end

n = nparms;
sz = cell(n,1);

for j=1:n
   sz{j} = size(varargin{j});
end
t = sz(~isscalar);
size1 = t{1};

% Scalars receive this size.  Other arrays must have the proper size.
for j=1:n
   sizej = sz{j};
   if (isscalar(j))
      vj = varargin{j};
      if isnumeric(vj)
         t = zeros(size1,class(vj));
      else
         t = zeros(size1);
      end
      t(:) = varargin{j};
      varargout{j} = t;
   elseif (~isequal(sizej,size1))
      errorcode = 1;
      return;
   end
end
